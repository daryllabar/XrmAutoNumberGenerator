using System;
using DLaB.Xrm.Plugin;

namespace DLaB.XrmAutoNumberGenerator
{
    public abstract class PluginHandlerBase : GenericPluginHandlerBase<LocalPluginContext>
    {
        protected override LocalPluginContext CreateLocalPluginContext(IServiceProvider serviceProvider)
        {
            return new LocalPluginContext(serviceProvider, this);
        }
    }
}
