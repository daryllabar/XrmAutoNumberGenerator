using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Plugin;

namespace DLaB.XrmAutoNumberGenerator
{
    public class LocalPluginContext : DLaBExtendedPluginContextBase
    {
        private IRegisteredEventsPluginHandler Plugin { get; }

        public LocalPluginContext(IServiceProvider serviceProvider, IRegisteredEventsPluginHandler plugin) : base(serviceProvider, plugin)
        {
            Plugin = plugin;
        }

        public override RegisteredEvent Event
        {
            get
            {
                var @event = base.Event ?? this.GetEvent(Plugin.RegisteredEvents
                                 .Where(e => e.Message == PluginHandlerBase.Any)
                                 .Select(e => new RegisteredEvent(e.Stage, this.GetMessageType(), e.Execute, e.EntityLogicalName)));

                return @event;
            }
        }
    }
}
