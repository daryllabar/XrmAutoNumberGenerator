using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm;
using DLaB.Xrm.Plugin;
using Microsoft.Xrm.Sdk;
using DLaB.Common;
using DLaB.XrmAutoNumberGenerator.Entities;

namespace DLaB.XrmAutoNumberGenerator
{
    public class AutoNumberIncrementor : PluginBase
    {
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
            return new AutoNumberIncrementorLogic();
        }
    }

    internal class AutoNumberIncrementorLogic : PluginHandlerBase
    {
        private readonly object _settingLock = new object();
        private static ConcurrentDictionary<string, AutoNumberManager[]> AutoNumberManagersByEntity { get; set; }

        static AutoNumberIncrementorLogic()
        {
            AutoNumberManagersByEntity = new ConcurrentDictionary<string, AutoNumberManager[]>();
        }

        public override void RegisterEvents()
        {
            RegisteredEvents.AddRange(new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.Create).Build());
        }

        protected override void ExecuteInternal(LocalPluginContext context)
        {
            var target = context.GetTarget<Entity>();
            var autoNumberManagers = AutoNumberManagersByEntity.GetOrAddSafe(_settingLock, target.LogicalName,
                logicalName =>
                    context.SystemOrganizationService.GetEntities<dlab_AutoNumbering>(
                        dlab_AutoNumbering.Fields.dlab_EntityName, logicalName).
                        Select(s => new AutoNumberManager(s)).ToArray());

            if (autoNumberManagers.Length == 0)
            {
                throw new InvalidPluginExecutionException("No Auto-Number Settings found for Entity " +
                                                          target.LogicalName);
            }

            // ReSharper disable once ForCanBeConvertedToForeach - Not sure if a foreach enumeration is thread safe
            for (var i = 0; i < autoNumberManagers.Length; i++)
            {
                var manager = autoNumberManagers[i];
                // Lock here so the settings entity won't be updated in the middle of processing the record
                lock (manager)
                {
                    SetAutoNumber(context, target, manager);
                }
            }
        }

        private void SetAutoNumber(LocalPluginContext context, Entity target, AutoNumberManager manager)
        {
            var setting = manager.Setting;

            if (setting.UseInitializedValue(target))
            {
                context.TraceFormat(
                    "The Attribute {0} in the Entity {1} already contains a value, and will not be overriden",
                    setting.AttributeName, setting.EntityName);
                return;
            }

            // Attempt to use Batched Auto Number
            if (manager.AutoNumberBatch.Count == 0)
            {
                context.TraceFormat("No batched value found.  Repopulating batch for {0}.{1}", setting.EntityName,
                    setting.AttributeName);
                manager.EnqueueBatch(context);

                if (manager.AutoNumberBatch.Count == 0)
                {
                    throw new InvalidPluginExecutionException("EnqueueBatch never enqueued a new value!");
                }
            }

            target[setting.AttributeName] = manager.AutoNumberBatch.Dequeue();
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
                Setting = dlab_AutoNumbering.EnqueueNextBatch(context.SystemOrganizationService, Setting, AutoNumberBatch);
            }
        }

    }
}
