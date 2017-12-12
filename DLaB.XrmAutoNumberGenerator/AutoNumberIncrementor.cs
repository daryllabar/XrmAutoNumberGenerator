using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DLaB.Xrm;
using DLaB.Xrm.Plugin;
using Microsoft.Xrm.Sdk;
using DLaB.Common;
using DLaB.XrmAutoNumberGenerator.Entities;

namespace DLaB.XrmAutoNumberGenerator
{
    public class AutoNumberIncrementor : PluginBase
    {
        /// <summary>
        /// For Unit Testing only
        /// </summary>
        public bool DisallowStaticCache { get; set; }

        #region Constructors

        public AutoNumberIncrementor() : this(null, null)
        {
        }

        public AutoNumberIncrementor(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig)
        {
        }

        #endregion Constructors

        protected override PluginHandlerBase GetPluginHandler()
        {
            return new AutoNumberIncrementorLogic(DisallowStaticCache);
        }
    }

    internal class AutoNumberIncrementorLogic : PluginHandlerBase
    {
        private readonly bool _disallowStaticCache;
        private readonly object _settingLock = new object();
        private static ConcurrentDictionary<string, AutoNumberManager[]> AutoNumberManagersByEntity { get; }

        static AutoNumberIncrementorLogic()
        {
            AutoNumberManagersByEntity = new ConcurrentDictionary<string, AutoNumberManager[]>();
        }

        public AutoNumberIncrementorLogic(bool disallowStaticCache)
        {
            _disallowStaticCache = disallowStaticCache;
        }

        protected override void PostExecute(IExtendedPluginContext context)
        {
            if (_disallowStaticCache)
            {
                AutoNumberManagersByEntity.Clear();
            }
            base.PostExecute(context);
        }

        public override void RegisterEvents()
        {
            RegisteredEvents.AddRange(new RegisteredEventBuilder(PipelineStage.PreValidation, Any).Build());
            RegisteredEvents.AddRange(new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.Create)
                .WithExecuteAction(ExecuteInTransactionForCustomEvent).Build());
        }

        private string GetKey(LocalPluginContext context, string name)
        {
            return $"{context.PluginTypeName}|{name}";
        }

        protected override void ExecuteInternal(LocalPluginContext context)
        {
            if (string.IsNullOrWhiteSpace(UnsecureConfig))
            {
                ExecuteForEntity(context, context.GetTarget<Entity>());
            }
            else if (context.IsInTransaction)
            {
                ExecuteInTransactionForCustomEvent(context);
            }
            else { 
                foreach (var logicalName in UnsecureConfig.Split(',', '|'))
                {
                    ExecuteBeforeTransactionForCustomEvent(context, logicalName);
                }
            }
        }

        private AutoNumberManager[] GetAutoNumberManagers(LocalPluginContext context, string logicalName)
        {
            var autoNumberManagers = AutoNumberManagersByEntity.GetOrAddSafe(_settingLock, logicalName,
                n => context.SystemOrganizationService
                    .GetEntities<dlab_AutoNumbering>(dlab_AutoNumbering.Fields.dlab_EntityName, n)
                    .Select(s => new AutoNumberManager(s)).ToArray());

            if (autoNumberManagers.Length == 0)
            {
                throw new InvalidPluginExecutionException("No Auto-Number Settings found for Entity " + logicalName);
            }

            return autoNumberManagers;
        }

        private void ExecuteInTransactionForCustomEvent(LocalPluginContext context)
        {
            var managers = GetAutoNumberManagers(context, context.PrimaryEntityName);
            var target = context.GetTarget<Entity>();
            // ReSharper disable once ForCanBeConvertedToForeach - Not sure if a foreach enumeration is thread safe
            for (var i = 0; i < managers.Length; i++)
            {
                var manager = managers[i];
                // Lock here so the settings entity won't be updated in the middle of processing the record
                lock (manager)
                {
                    if (manager.Setting.UseInitializedValue(target))
                    {
                        context.Trace(manager.Setting.FullName + " already contains a value, and will not be overriden");
                        continue;
                    }
                }

                var value = context.GetFirstSharedVariable<string>(GetKey(context, manager.Setting.FullName));
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (UnsecureConfig?.Contains("AllowInTransactionGeneration") == true)
                    {
                        value = GenerateAutoNumber(context, manager);
                        context.Trace($"Value {value} generated for Pre-Operation, setting {manager.Setting.FullName}.");
                        target[manager.Setting.AttributeName] = value;
                    }
                    else
                    {
                        context.Trace("No value found for Pre-Operation, unable to set " + manager.Setting.FullName);
                        context.Trace("This is normally due to another plugin triggering the create of the entity, and the prevalidation either not running, or running in the context of a transaction.");
                        context.Trace("Manually register the AutoNumberIncrementor for the parent event/action.");
                        context.Trace(context.GetContextInfo());
                    }
                }
                else
                {
                    context.Trace($"Value {value} found for Pre-Operation, setting {manager.Setting.FullName}.");
                    target[manager.Setting.AttributeName] = value;
                }
            }
        }

        private void ExecuteBeforeTransactionForCustomEvent(LocalPluginContext context, string logicalName)
        {
            var managers = GetAutoNumberManagers(context, logicalName.ToLower());

            // ReSharper disable once ForCanBeConvertedToForeach - Not sure if a foreach enumeration is thread safe
            for (var i = 0; i < managers.Length; i++)
            {
                var manager = managers[i];
                // Lock here so the settings entity won't be updated in the middle of processing the record
                lock (manager)
                {
                    var number = GenerateAutoNumber(context, manager);
                    context.SharedVariables[GetKey(context, manager.Setting.FullName)] = number;
                }
            }
        }

        private void ExecuteForEntity(LocalPluginContext context, Entity target)
        {
            var autoNumberManagers = GetAutoNumberManagers(context, target.LogicalName);

            // ReSharper disable once ForCanBeConvertedToForeach - Not sure if a foreach enumeration is thread safe
            for (var i = 0; i < autoNumberManagers.Length; i++)
            {
                var manager = autoNumberManagers[i];
                // Lock here so the settings entity won't be updated in the middle of processing the record
                lock (manager)
                {
                    if (manager.Setting.UseInitializedValue(target))
                    {
                        context.Trace(manager.Setting.FullName + " already contains a value, and will not be overriden");
                        continue;
                    }

                    target[manager.Setting.AttributeName] = GenerateAutoNumber(context, manager);
                }
            }
        }

        private static string GenerateAutoNumber(LocalPluginContext context, AutoNumberManager manager)
        {
            // Attempt to use Batched Auto Number
            if (manager.AutoNumberBatch.Count == 0)
            {
                context.Trace("No batched value found.  Repopulating batch for " + manager.Setting.FullName);
                manager.EnqueueBatch(context);

                if (manager.AutoNumberBatch.Count == 0)
                {
                    throw new InvalidPluginExecutionException("EnqueueBatch never enqueued a new value!");
                }
            }
            var number = manager.AutoNumberBatch.Dequeue();
            context.Trace($"Value '{number}' dequeued from generated values.");
            return number;
        }

        private class AutoNumberManager
        {
            public dlab_AutoNumbering Setting { get; set; }
            public Queue<string> AutoNumberBatch { get; set; }

            public AutoNumberManager(dlab_AutoNumbering setting)
            {
                Setting = setting;
                AutoNumberBatch = new Queue<string>();
            }

            /// <summary>
            /// Triggers the next number generation.  Updates the Setting of the manager
            /// </summary>
            /// <param name="context">Plugin Context</param>
            public void EnqueueBatch(LocalPluginContext context)
            {
                var count = 1;
                while (true)
                {
                    try
                    {
                        Setting = dlab_AutoNumbering.EnqueueNextBatch(context.SystemOrganizationService, Setting, AutoNumberBatch, context.TracingService, context.IsInTransaction);
                        context.Trace("Successfully enqueued batch");
                        break;
                    }
                    catch (FaultException ex)
                    {
                        // Only Retry if the Error contains the Mult-ThreadedError
                        if (ex.Message.Contains(AutoNumberRegister.MultiThreadedErrorMessage))
                        {
                            //Reload Setting
                            context.Trace("Conflict Id found.  Retry #" + count++);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Trace("An unexpected exception occured of type " + ex.GetType().FullName);
                        context.Trace("Message: " + ex.Message);
                        throw;
                    }
                }
            }
        }

    }
}
