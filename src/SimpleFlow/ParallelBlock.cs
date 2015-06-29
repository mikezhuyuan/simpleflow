using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFlow.Core
{
    class ParallelBlock : GroupBlock
    {
        public ParallelBlock(IEnumerable<WorkflowBlock> children, int maxWorkers)
        {
            if (children == null) throw new ArgumentNullException("children");
            if (maxWorkers <= 0) throw new ArgumentException("maxWorkers must be a positive number");

            var c = children.ToArray();
            var inputType = c.First().InputTypes.SingleOrDefault();

            if (inputType == null && c.Any(_ => _.InputTypes.Any()))
                throw new Exception("type are not same over all child items");

            if (inputType != null &&
                c.Any(_ => !_.InputTypes.Any() || !_.InputTypes.Single().IsAssignableFrom(inputType)))
                throw new Exception("type are not same over all child items"); //todo: simplify logic

            InputTypes = inputType == null ? Enumerable.Empty<Type>() : new[] {inputType};
            Children = c;
            MaxWorkers = maxWorkers;

            OutputType = c.All(_ => _.OutputType == typeof (void)) ? typeof (void) : typeof (IEnumerable<object>);
        }

        public int MaxWorkers { get; set; }

        public override WorkflowType Type
        {
            get { return WorkflowType.Parallel; }
        }
    }
}