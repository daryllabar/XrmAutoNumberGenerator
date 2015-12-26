using System;
using DLaB.Xrm.Plugin;

namespace DLaB.XrmAutoNumberGenerator
{
    public static class Extensions
    {
        #region RegisteredEventBuilder

        public static RegisteredEventBuilder WithExecuteAction(this RegisteredEventBuilder builder, Action<LocalPluginContext> execute)
        {
            builder.WithExecuteAction(execute);
            return builder;
        }

        #endregion RegisteredEventBuilder
    }
}
