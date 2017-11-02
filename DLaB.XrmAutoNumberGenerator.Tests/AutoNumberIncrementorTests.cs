using System;
using System.Linq;
using DLaB.Xrm;
using DLaB.Xrm.Plugin;
using DLaB.Xrm.Test;
using DLaB.XrmAutoNumberGenerator.Entities;
using DLaB.XrmAutoNumberGenerator.Test;
using DLaB.XrmAutoNumberGenerator.Test.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.XrmAutoNumberGenerator.Tests
{
    [TestClass]
    public class AutoNumberIncrementorTests
    {
        #region CustomRegistration_Should_AddValueToContext

        [TestMethod]
        public void AutoNumberIncrementor_CustomRegistration_Should_AddValueToContext()
        {
            new CustomRegistration_Should_AddValueToContext().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class CustomRegistration_Should_AddValueToContext : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<dlab_AutoNumbering> AutoNumbering = new Id<dlab_AutoNumbering>("44AF771F-61E4-407F-A141-798B5490FFB6");

            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var plugin = new AutoNumberIncrementor(SystemUser.EntityLogicalName, null) { DisallowStaticCache = true };
                var context = new PluginExecutionContextBuilder()
                    .WithRegisteredEvent(new RegisteredEvent(PipelineStage.PreValidation, MessageType.QualifyLead)).Build();
                var serviceProvider = new ServiceProviderBuilder(service, context, Logger).Build();

                //
                // Act
                //
                plugin.Execute(serviceProvider);

                //
                // Assert
                //
                context = serviceProvider.GetService<IPluginExecutionContext>();
                var value = context.SharedVariables.Last();
                Assert.AreEqual(String.Format(Ids.AutoNumbering.Entity.dlab_name, Ids.AutoNumbering.Entity.dlab_NextNumber), value.Value);
            }
        }

        #endregion CustomRegistration_Should_AddValueToContext

        #region CustomRegistration_Should_UseValueAddedToContext

        [TestMethod]
        public void AutoNumberIncrementor_CustomRegistration_Should_UseValueAddedToContext()
        {
            new CustomRegistration_Should_UseValueAddedToContext().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class CustomRegistration_Should_UseValueAddedToContext : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<dlab_AutoNumbering> AutoNumbering = new Id<dlab_AutoNumbering>("D754A306-D2FD-41F0-B191-A748646A6D3E");
            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                service = new OrganizationServiceBuilder(service).IsReadOnly().Build();
                var auto = Ids.AutoNumbering.Entity;
                var target = new Entity(auto.EntityName);
                var nextValue = string.Format(auto.dlab_name, auto.dlab_NextNumber);
                var plugin = new AutoNumberIncrementor(SystemUser.EntityLogicalName, null) {DisallowStaticCache = true};
                var context = new PluginExecutionContextBuilder()
                    .WithRegisteredEvent(new RegisteredEvent(PipelineStage.PreOperation, MessageType.Create))
                    .WithTarget(target)
                    .WithSharedVariable(GetSharedVariableKey(plugin, auto), nextValue).Build();
                var serviceProvider = new ServiceProviderBuilder(service, context, Logger).Build();

                //
                // Act
                //
                plugin.Execute(serviceProvider);

                //
                // Assert
                //
                Assert.AreEqual(target[auto.AttributeName], nextValue);
            }
        }

        #endregion CustomRegistration_Should_UseValueAddedToContext

        #region DefaultRegistrationInTransaction_Should_UseValueAddedToContext

        [TestMethod]
        public void AutoNumberIncrementor_DefaultRegistrationInTransaction_Should_UseValueAddedToContext()
        {
            new DefaultRegistrationInTransaction_Should_UseValueAddedToContext().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class DefaultRegistrationInTransaction_Should_UseValueAddedToContext : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<dlab_AutoNumbering> AutoNumbering = new Id<dlab_AutoNumbering>("0923E803-DD6D-4888-BA13-0939E096A2B0");
            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                service = new OrganizationServiceBuilder(service).IsReadOnly().Build();
                var auto = Ids.AutoNumbering.Entity;
                var target = new Entity(auto.EntityName);
                var nextValue = string.Format(auto.dlab_name, auto.dlab_NextNumber);
                var plugin = new AutoNumberIncrementor(SystemUser.EntityLogicalName, null) { DisallowStaticCache = true };
                var context = new PluginExecutionContextBuilder()
                    .InTranasction()
                    .WithRegisteredEvent(new RegisteredEvent(PipelineStage.PreValidation, MessageType.Create))
                    .WithTarget(target)
                    .WithSharedVariable(GetSharedVariableKey(plugin, auto), nextValue).Build();
                var serviceProvider = new ServiceProviderBuilder(service, context, Logger).Build();

                //
                // Act
                //
                plugin.Execute(serviceProvider);

                //
                // Assert
                //
                Assert.AreEqual(target[auto.AttributeName], nextValue);
            }
        }

        #endregion DefaultRegistrationInTransaction_Should_UseValueAddedToContext

        #region DefaultRegistrationWithNoSharedVariable_Should_GenerateNewAutoNum

        [TestMethod]
        public void AutoNumberIncrementor_DefaultRegistrationWithNoSharedVariable_Should_GenerateNewAutoNum()
        {
            new DefaultRegistrationWithNoSharedVariable_Should_GenerateNewAutoNum().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class DefaultRegistrationWithNoSharedVariable_Should_GenerateNewAutoNum : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<dlab_AutoNumbering> AutoNumbering = new Id<dlab_AutoNumbering>("BE81BD73-5197-4CF2-AFAB-C2F7793A062F");
            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var auto = Ids.AutoNumbering.Entity;
                var target = new Entity(auto.EntityName);
                var nextValue = string.Format(auto.dlab_name, auto.dlab_NextNumber);
                var plugin = new AutoNumberIncrementor { DisallowStaticCache = true };
                var context = new PluginExecutionContextBuilder()
                    .WithRegisteredEvent(new RegisteredEvent(PipelineStage.PreValidation, MessageType.Create))
                    .WithTarget(target).Build();
                var serviceProvider = new ServiceProviderBuilder(service, context, Logger).Build();

                //
                // Act
                //
                plugin.Execute(serviceProvider);

                //
                // Assert
                //
                Assert.AreEqual(target[auto.AttributeName], nextValue);
            }
        }

        #endregion DefaultRegistrationWithNoSharedVariable_Should_GenerateNewAutoNum

        //#region DefaultRegistrationWithSharedVariable_Should_UseValueAddedToContext
        //
        //[TestMethod]
        //public void PluginName_DefaultRegistrationWithSharedVariable_Should_UseValueAddedToContext()
        //{
        //    new DefaultRegistrationWithSharedVariable_Should_UseValueAddedToContext().Test();
        //}
        //
        //// ReSharper disable once InconsistentNaming
        //private class DefaultRegistrationWithSharedVariable_Should_UseValueAddedToContext : TestMethodClassBase
        //{
        //    private struct Ids
        //    {
        //        public static readonly Id<dlab_AutoNumbering> AutoNumbering = new Id<dlab_AutoNumbering>("71D7920D-3316-4B42-8628-D56990C70E73");
        //    }
        //
        //    protected override void InitializeTestData(IOrganizationService service)
        //    {
        //        new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
        //    }
        //
        //    protected override void Test(IOrganizationService service)
        //    {
        //        //
        //        // Arrange
        //        //
        //        service = new OrganizationServiceBuilder(service).IsReadOnly().Build();
        //        var auto = Ids.AutoNumbering.Entity;
        //        var target = new Entity(auto.EntityName);
        //        var nextValue = auto.dlab_name;
        //        var plugin = new AutoNumberIncrementor { DisallowStaticCache = true };
        //        var context = new PluginExecutionContextBuilder()
        //            .WithRegisteredEvent(new RegisteredEvent(PipelineStage.PreValidation, MessageType.Create))
        //            .WithSharedVariable(GetSharedVariableKey(plugin, auto), nextValue)
        //            .WithTarget(target).Build();
        //        var serviceProvider = new ServiceProviderBuilder(service, context, Logger).Build();
        //
        //        //
        //        // Act
        //        //
        //        plugin.Execute(serviceProvider);
        //
        //        //
        //        // Assert
        //        //
        //        Assert.AreEqual(target[auto.AttributeName], nextValue);
        //    }
        //}
        //
        //#endregion DefaultRegistrationWithSharedVariable_Should_UseValueAddedToContext

        #region InitializationAllowed_Should_NotOverride

        [TestMethod]
        public void AutoNumberIncrementor_InitializationAllowed_Should_NotOverride()
        {
            new InitializationAllowed_Should_NotOverride().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class InitializationAllowed_Should_NotOverride : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<dlab_AutoNumbering> AutoNumbering = new Id<dlab_AutoNumbering>("31C90071-F412-4467-9167-90AE31168E3E");
            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                Ids.AutoNumbering.Entity.dlab_AllowExternalInitialization = true;
                new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                service = new OrganizationServiceBuilder(service).IsReadOnly().Build();
                var auto = Ids.AutoNumbering.Entity;
                var target = new Entity(auto.EntityName);
                target[auto.AttributeName] = auto.dlab_name;
                var plugin = new AutoNumberIncrementor { DisallowStaticCache = true };
                var context = new PluginExecutionContextBuilder()
                    .WithRegisteredEvent(new RegisteredEvent(PipelineStage.PreValidation, MessageType.Create))
                    .WithTarget(target).Build();
                var serviceProvider = new ServiceProviderBuilder(service, context, Logger).Build();

                //
                // Act
                //
                plugin.Execute(serviceProvider);

                //
                // Assert
                //
                Assert.AreEqual(target[auto.AttributeName], auto.dlab_name);
            }
        }

        #endregion InitializationAllowed_Should_NotOverride

        #region InitializationNotAllowed_Should_Override

        [TestMethod]
        public void AutoNumberIncrementor_InitializationNotAllowed_Should_Override()
        {
            new InitializationNotAllowed_Should_Override().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class InitializationNotAllowed_Should_Override : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<dlab_AutoNumbering> AutoNumbering = new Id<dlab_AutoNumbering>("08CC5F52-16FC-4EEA-9492-43D516E9EFAF");
            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                Ids.AutoNumbering.Entity.dlab_AllowExternalInitialization = false;
                new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var auto = Ids.AutoNumbering.Entity;
                var target = new Entity(auto.EntityName);
                target[auto.AttributeName] = auto.dlab_name;
                var nextValue = string.Format(auto.dlab_name, auto.dlab_NextNumber);
                var plugin = new AutoNumberIncrementor { DisallowStaticCache = true };
                var context = new PluginExecutionContextBuilder()
                    .WithRegisteredEvent(new RegisteredEvent(PipelineStage.PreValidation, MessageType.Create))
                    .WithTarget(target).Build();
                var serviceProvider = new ServiceProviderBuilder(service, context, Logger).Build();

                //
                // Act
                //
                plugin.Execute(serviceProvider);

                //
                // Assert
                //
                Assert.AreEqual(target[auto.AttributeName], nextValue);
            }
        }

        #endregion InitializationNotAllowed_Should_Override

        private static string GetSharedVariableKey(AutoNumberIncrementor plugin, dlab_AutoNumbering auto)
        {
            return $"{plugin.GetType()}Logic|{auto.EntityName}.{auto.AttributeName}";
        }
    }
}
