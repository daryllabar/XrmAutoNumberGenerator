﻿using System.Activities;

namespace DLaB.XrmAutoNumberGenerator.Test.Builders
{
    public class WorkflowInvokerBuilder : DLaB.Xrm.Test.Builders.WorkflowInvokerBuilderBase<WorkflowInvokerBuilder>
    {
        protected override WorkflowInvokerBuilder This => this;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowInvokerBuilder"/> class.
        /// </summary>
        /// <param name="workflow">The workflow to invoke.</param>
        public WorkflowInvokerBuilder(Activity workflow) : base(workflow)
        {

        }
    }
}
