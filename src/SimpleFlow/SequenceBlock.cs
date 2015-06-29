using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFlow.Core
{
    class SequenceBlock : GroupBlock
    {
        public SequenceBlock(IEnumerable<WorkflowBlock> children)
        {
            if (children == null) throw new ArgumentNullException("children");
            var copy = children.ToArray();
            if (!copy.Any()) throw new ArgumentException("child items cannot be empty");

            Children = copy;
            InputTypes = copy.First().InputTypes;
            OutputType = copy.Last().OutputType;
        }

        public override WorkflowType Type
        {
            get { return WorkflowType.Sequence; }
        }
    }
}