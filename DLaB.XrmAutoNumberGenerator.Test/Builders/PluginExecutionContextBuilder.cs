namespace DLaB.XrmAutoNumberGenerator.Test.Builders
{
    public class PluginExecutionContextBuilder : DLaB.Xrm.Test.Builders.PluginExecutionContextBuilderBase<PluginExecutionContextBuilder>
    {
        protected override PluginExecutionContextBuilder This => this;

        #region Fluent Methods

        public PluginExecutionContextBuilder InTranasction()
        {
            Context.IsInTransaction = true;
            return this;
        }

        #endregion Fluent Methods
    }
}
