using System;
using System.Reflection;
using DLaB.Common.Exceptions;
using DLaB.Xrm;
using DLaB.Xrm.Plugin;
using DLaB.XrmAutoNumberGenerator.Entities;
using Microsoft.Xrm.Sdk;

namespace DLaB.XrmAutoNumberGenerator
{
    public class AutoNumberRegister : PluginBase
    {
        public AutoNumberRegister(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig) { }
        protected override PluginHandlerBase GetPluginHandler()
        {
            return new AutoNumberRegisterLogic();
        }
    }

    internal class AutoNumberRegisterLogic : PluginHandlerBase
    {
        public override void RegisterEvents()
        {
            RegisteredEvents.AddRange(new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.Create).
                                          ForEntities(dlab_AutoNumbering.EntityLogicalName).
                                          WithExecuteAction(RegisterIncrementor).Build());
            RegisteredEvents.AddRange(new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.Delete).
                                          ForEntities(dlab_AutoNumbering.EntityLogicalName).
                                          WithExecuteAction(UnregisterIncrementor).Build());
            RegisteredEvents.AddRange(new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.Update).
                                          ForEntities(dlab_AutoNumbering.EntityLogicalName).
                                          WithExecuteAction(ProcessStateChange).Build());
        }

        protected override void ExecuteInternal(LocalPluginContext context)
        {
            throw new NotImplementedException("Plugin is designed to only work for ProcessStateChange, RegisterIncrementor, and UnregisterIncrementor Methods");
        }

        protected void RegisterIncrementor(LocalPluginContext context)
        {
            var settings = context.CoallesceTargetWithPreEntity<dlab_AutoNumbering>();
            var createMessage = GetCreateMessageId(context);

            var messageProcessingStep = new SdkMessageProcessingStep
            {
                ModeEnum = sdkmessageprocessingstep_mode.Synchronous,
                Name = $"IdGenerator for the {settings.dlab_EntityName} Entity",
                Rank = settings.dlab_PluginExecutionOrder.GetValueOrDefault(1),
                SdkMessageId = createMessage,
                SdkMessageFilterId = GetMessageFilterId(context, settings.EntityName, createMessage.Id),
                EventHandler = GetPluginId(context),
                StageEnum = sdkmessageprocessingstep_stage.Preoperation,
                SupportedDeploymentEnum = sdkmessageprocessingstep_supporteddeployment.ServerOnly
            };

            var target = context.GetTarget<dlab_AutoNumbering>();
            target.dlab_PluginStepId = context.SystemOrganizationService.Create(messageProcessingStep).ToString();
        }

        protected void UnregisterIncrementor(LocalPluginContext context)
        {
            var settings = context.OrganizationService.GetEntity<dlab_AutoNumbering>(context.PrimaryEntity.Id);
            Guid pluginStepId;
            if (string.IsNullOrWhiteSpace(settings.dlab_PluginStepId) || !Guid.TryParse(settings.dlab_PluginStepId, out pluginStepId))
            {
                return;
            }
            context.SystemOrganizationService.TryDelete(SdkMessageProcessingStep.EntityLogicalName, pluginStepId);

            if (context.Event.Message == MessageType.Delete)
            {
                return;
            }

            // Could be called from deactivate update.  Clear Plugin Setp
            var target = context.GetTarget<dlab_AutoNumbering>();
            target.dlab_PluginStepId = null;
        }

        protected void ProcessStateChange(LocalPluginContext context)
        {
            var target = context.GetTarget<dlab_AutoNumbering>();
            switch (target.statecode)
            {
                case dlab_AutoNumberingState.Active:
                    // Register Plugin if it doesn't exist
                    var settings = context.OrganizationService.GetEntity<dlab_AutoNumbering>(context.PrimaryEntity.Id);
                    if (settings.dlab_PluginStepId == null || context.SystemOrganizationService.GetEntityOrDefault<SdkMessageProcessingStep>(Guid.Parse(settings.dlab_PluginStepId)) == null)
                    {
                        RegisterIncrementor(context);
                    }
                    break;
                case dlab_AutoNumberingState.Inactive:
                    // Unregister Plugin
                    UnregisterIncrementor(context);
                    break;
                case null:
                    break;
                default:
                    throw new EnumCaseUndefinedException<dlab_AutoNumberingState>(target.statecode.GetValueOrDefault());
            }
        }

        private EntityReference GetCreateMessageId(LocalPluginContext context)
        {
            var message = context.SystemOrganizationService.GetFirst<SdkMessage>(m => new { m.Id }, SdkMessage.Fields.Name, "Create");
            return message.ToEntityReference();
        }

        private EntityReference GetMessageFilterId(LocalPluginContext context, string entityName, Guid sdkMessage)
        {
            var filter = context.SystemOrganizationService.GetFirst<SdkMessageFilter>(f => new { f.Id }, SdkMessageFilter.Fields.PrimaryObjectTypeCode, entityName, SdkMessageFilter.Fields.SdkMessageId, sdkMessage);
            return filter.ToEntityReference();
        }

        private EntityReference GetPluginId(LocalPluginContext context)
        {
            var name = Assembly.GetExecutingAssembly().FullName;
            name = name.Substring(0, name.IndexOf(','));
            var plugin = context.SystemOrganizationService.GetFirst<PluginType>(p => new { p.Id },
                PluginType.Fields.AssemblyName, name,
                PluginType.Fields.TypeName, typeof(AutoNumberIncrementor).FullName);
            return plugin.ToEntityReference();
        }
    }
}
