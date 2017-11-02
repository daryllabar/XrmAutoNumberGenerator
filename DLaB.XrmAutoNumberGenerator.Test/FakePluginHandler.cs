using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Plugin;

namespace DLaB.XrmAutoNumberGenerator.Test
{
    public class FakePluginHandler : IRegisteredEventsPluginHandler
    {
        public List<RegisteredEvent> RegisteredEvents => new List<RegisteredEvent>();

        public void Execute(IServiceProvider serviceProvider)
        {
            
        }

        public void RegisterEvents()
        {
            
        }

        public void SetConfigValues(string unsecureConfig = null, string secureConfig = null)
        {
            
        }
    }
}
