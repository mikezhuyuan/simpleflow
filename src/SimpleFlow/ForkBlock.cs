using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFlow.Core
{
    class ForkBlock : GroupBlock
    {
        public ForkBlock(WorkflowBlock child, int maxWorkers)
        {
            if (child == null) throw new ArgumentNullException("child");
            if (maxWorkers <= 0) throw new ArgumentException("maxWorkers must be a positive number");

            InputTypes = child.InputTypes;
            if (child.OutputType == typeof (void))
            {
                OutputType = typeof (void);
            }
            else
            {
                OutputType = typeof (IEnumerable<>).MakeGenericType(child.OutputType);
            }

            Child = child;
            MaxWorkers = maxWorkers;
        }

        public WorkflowBlock Child
        {
            get { return Children.Single(); }
            set { Children = new[] {value}; }
        }

        public int MaxWorkers { get; set; }

        public override WorkflowType Type
        {
            get { return WorkflowType.Fork; }
        }
    }
}