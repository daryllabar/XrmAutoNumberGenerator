using System;
using DLaB.Xrm.Test;
using DLaB.XrmAutoNumberGenerator.Entities;

namespace DLaB.XrmAutoNumberGenerator.Test.Builders
{
    public class AutoNumberingBuilder: EntityBuilder<dlab_AutoNumbering>
    {
        public dlab_AutoNumbering AutoNumber { get; set; }

        public AutoNumberingBuilder()
        {
            AutoNumber = new dlab_AutoNumbering
            {
                dlab_AllowExternalInitialization = true,
                dlab_AttributeName = SystemUser.Fields.LastName,
                dlab_EntityName = SystemUser.EntityLogicalName,
                dlab_FixedNumberSize = 8,
                dlab_IncrementStepSize = 1,
                dlab_NextNumber = 1,
                dlab_PadWithZeros = true,
                dlab_PluginExecutionOrder = 1,
                dlab_Postfix = "-Post",
                dlab_PluginStepId = Guid.NewGuid().ToString(),
                dlab_Prefix = "Pre-",
                dlab_ServerBatchSize = 1,
                dlab_name = "Pre-0000000{0}-Post"
            };
        }

        public AutoNumberingBuilder(Id id) : this()
        {
            Id = id;
        }

        #region Fluent Methods

        #endregion Fluent Methods

        protected override dlab_AutoNumbering BuildInternal()
        {
            return AutoNumber;
        }
    }
}
