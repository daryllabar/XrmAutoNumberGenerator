using DLaB.Xrm.Test;
using Microsoft.Xrm.Sdk;

namespace DLaB.XrmAutoNumberGenerator.Test.Builders
{
    public class OrganizationServiceBuilder : DLaB.Xrm.Test.Builders.OrganizationServiceBuilderBase<OrganizationServiceBuilder>
    {
        protected override OrganizationServiceBuilder This => this;

        #region Constructors


        public OrganizationServiceBuilder() : this(TestBase.GetOrganizationService()) {}

        public OrganizationServiceBuilder(IOrganizationService service) : base(service) {}

        #endregion Constructors
    }
}