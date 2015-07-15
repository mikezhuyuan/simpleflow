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
                BuildBlocks = () => new ActivityBlock(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ForkJoin<TInput, TOutput> ForEach<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new ForkJoin<TInput, TOutput>
            {
                BuildBlocks = () => new ActivityBlock(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ForkJoin<TInput, TOutput> ForEach<TOutput>(Workflow<TInput, TOutput> workflow)
        {
            return new ForkJoin<TInput, TOutput>
            {
                BuildBlocks = () => workflow.BuildBlock(),
                MaxWorkers = MaxWorkers
            };
        }
    }

    public class ForkJoin<TInput, TOutput>
    {
        internal int MaxWorkers = 1;
        internal Func<WorkflowBlock> BuildBlocks { get; set; } //todo: BuildWorkflow? maybe too verbose

        public Workflow<IEnumerable<TInput>, IEnumerable<TOutput>> Join()
        {
            return new Workflow<IEnumerable<TInput>, IEnumerable<TOutput>>
            {
                BuildBlock = () => new ForkBlock(BuildBlocks(), MaxWorkers)
            };
        }
    }
}