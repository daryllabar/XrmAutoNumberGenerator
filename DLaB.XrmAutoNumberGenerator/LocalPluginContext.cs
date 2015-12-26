using System;
using DLaB.Xrm.Plugin;

namespace DLaB.XrmAutoNumberGenerator
{
    public class LocalPluginContext : LocalPluginContextBase
    {
        public LocalPluginContext(IServiceProvider serviceProvider, IRegisteredEventsPluginHandler plugin) : base(serviceProvider, plugin)
        {
        }
    }
}
