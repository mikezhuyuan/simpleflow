using System;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public class WorkflowBuilder<TInput, TOutput>
    {
        internal Func<WorkflowBlock> BuildWorkflow { get; set; }

        public SimpleFlow.Core.Workflow<TInput, TOutput> Build(string name)
        {
            var root = BuildWorkflow();

            return new SimpleFlow.Core.Workflow<TInput, TOutput>(name, root);
        }
    }
}