using System;
using System.Collections.Generic;
using DLaB.Xrm.Plugin;

namespace DLaB.XrmAutoNumberGenerator
{
    public abstract class PluginBase : IRegisteredEventsPlugin
    {
        private readonly object _handlerLock = new object();
        private PluginHandlerBase _handler;
        private volatile bool _isIntialized;

        private String SecureConfig { get; set; }
        private String UnsecureConfig { get; set; }
        public IEnumerable<RegisteredEvent> RegisteredEvents => ThreadSafeGetOrCreateHandler().RegisteredEvents;

        protected PluginBase(string unsecureConfig, string secureConfig)
        {
            UnsecureConfig = unsecureConfig;
            SecureConfig = secureConfig;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            ThreadSafeGetOrCreateHandler().Execute(serviceProvider);
        }

        private IRegisteredEventsPluginHandler ThreadSafeGetOrCreateHandler()
        {
            if (_handler != null) { return _handler; }

            if (_isIntialized) { return _handler; }

            lock (_handlerLock)
            {
                if (_isIntialized) { return _handler; }

                var local = GetPluginHandler();
                local.SetConfigValues(UnsecureConfig, SecureConfig);
                local.RegisterEvents();
                _handler = local;
                _isIntialized = true;
            }
            return _handler;
        }

        protected abstract PluginHandlerBase GetPluginHandler();
    }
}
