using System;
using System.Linq;

namespace SimpleFlow.Core
{
    public class RetryBlock : GroupBlock
    {
        public RetryBlock(WorkflowBlock child, int retryCount)
        {
            if (child == null) throw new ArgumentNullException("child");
            if (retryCount < 1) throw new ArgumentException("retryCount cannot be less than 1");

            Child = child;

            InputTypes = child.InputTypes;
            OutputType = child.OutputType;

            RetryCount = retryCount;
        }

        public WorkflowBlock Child
        {
            get { return Children.Single(); }
            set { Children = new[] { value }; }
        }

        public override WorkflowType Type
        {
            get { return WorkflowType.Retry; }
        }

        public int RetryCount { get; set; }
    }
}