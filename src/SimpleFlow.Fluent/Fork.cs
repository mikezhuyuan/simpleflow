using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public class ForkForEach<TInput>
    {
        internal int MaxWorkers = 1;

        public ForkJoin<TInput, TOutput> ForEach<TOutput>(Func<TInput, TOutput> func)
        {
            return new ForkJoin<TInput, TOutput>
            {
                BuildWorkflow = () => new ActivityBlock(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ForkJoin<TInput, TOutput> ForEach<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new ForkJoin<TInput, TOutput>
            {
                BuildWorkflow = () => new ActivityBlock(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ForkJoin<TInput, TOutput> ForEach<TOutput>(WorkflowBuilder<TInput, TOutput> workflowBuilder)
        {
            return new ForkJoin<TInput, TOutput>
            {
                BuildWorkflow = () => workflowBuilder.BuildWorkflow(),
                MaxWorkers = MaxWorkers
            };
        }
    }

    public class ForkJoin<TInput, TOutput>
    {
        internal int MaxWorkers = 1;
        internal Func<WorkflowBlock> BuildWorkflow { get; set; } //todo: BuildWorkflow? maybe too verbose

        public WorkflowBuilder<IEnumerable<TInput>, IEnumerable<TOutput>> Join()
        {
            return new WorkflowBuilder<IEnumerable<TInput>, IEnumerable<TOutput>>
            {
                BuildWorkflow = () => new ForkBlock(BuildWorkflow(), MaxWorkers)
            };
        }
    }
}