using System;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public class Workflow<TInput, TOutput>
    {
        internal Func<WorkflowBlock> BuildBlock { get; set; }

        public SimpleFlow.Core.Workflow<TInput, TOutput> Build(string name)
        {
            var root = BuildBlock();

            return new SimpleFlow.Core.Workflow<TInput, TOutput>(name, root);
        }
    }
}