using System;
using System.Linq;
using DLaB.Xrm.Plugin;

namespace DLaB.XrmAutoNumberGenerator
{
    public abstract class PluginHandlerBase : GenericPluginHandlerBase<LocalPluginContext>
    {
        public static MessageType Any = new MessageType(nameof(Any));

        protected override LocalPluginContext CreatePluginContext(IServiceProvider serviceProvider)
        {
            return new LocalPluginContext(serviceProvider, this);
        }
    }
}
