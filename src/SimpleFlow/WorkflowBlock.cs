using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFlow.Core
{
    public abstract class WorkflowBlock
    {
        public abstract WorkflowType Type { get; }
        public IEnumerable<Type> InputTypes { get; protected set; }
        public Type OutputType { get; protected set; }
        public Delegate ExceptionHandler { get; set; }
    }

    public abstract class GroupBlock : WorkflowBlock
    {
        protected GroupBlock()
        {
            Children = Enumerable.Empty<WorkflowBlock>();
        }

        public IEnumerable<WorkflowBlock> Children { get; protected set; }

        public override string ToString()
        {
            return Type + "(" + string.Join(",", Children.Select(_ => _.ToString())) + ")";
        }
    }

    /*
    class Retry : WorkflowItem
    {
        public WorkflowItem Child { get; internal set; }

        public int Count { get; internal set; }
    }

    class Pallaral : WorkflowItem
    {
        public WorkflowItem[] Children { get; internal set; }
    }
    */
}