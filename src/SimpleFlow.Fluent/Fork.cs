using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public class Fork<TInput>
    {
        internal int MaxWorkers;
    }

    public static class ForkExtension
    {
        public static ForkJoin<TInput, TOutput> ForEach<TInput, TOutput>(this Fork<IEnumerable<TInput>> source,
            Func<TInput, TOutput> func)
        {
            return new ForkJoin<TInput, TOutput>
            {
                BuildBlocks = () => new ActivityBlock(func),
                MaxWorkers = source.MaxWorkers
            };
        }

        public static ForkJoin<TInput, TOutput> ForEach<TInput, TOutput>(this Fork<IEnumerable<TInput>> source,
            Func<TInput, Task<TOutput>> func)
        {
            return new ForkJoin<TInput, TOutput>
            {
                BuildBlocks = () => new ActivityBlock(func),
                MaxWorkers = source.MaxWorkers
            };
        }

        public static ForkJoin<TInput, TOutput> ForEach<TInput, TOutput>(this Fork<IEnumerable<TInput>> source,
            Workflow<TInput, TOutput> workflow)
        {
            return new ForkJoin<TInput, TOutput>
            {
                BuildBlocks = () => workflow.BuildBlock(),
                MaxWorkers = source.MaxWorkers
            };
        }
    }

    public class ForkForEach<TInput>
    {
        internal int MaxWorkers;

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
        internal int MaxWorkers;
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