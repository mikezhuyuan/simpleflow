using System;
using System.Threading.Tasks;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public static class FluentFlow //todo: fluent flow is a good candidate for the whole project
    {
        public static SequenceThen<TInput, TInput> Sequence<TInput>()
        {
            return new SequenceThen<TInput, TInput>();
        }

        public static ParallelDo<TInput> Parallel<TInput>(int maxWorkers = 1)
        {
            return new ParallelDo<TInput>
            {
                MaxWorkers = maxWorkers
            };
        }

        public static Fork<TInput> Fork<TInput>(int maxWorkers = 1)
        {
            return new Fork<TInput>
            {
                MaxWorkers = maxWorkers
            };
        }

        public static Workflow<TInput, TOutput> Activity<TInput, TOutput>(Func<TInput, TOutput> func)
        {
            return new Workflow<TInput, TOutput>
            {
                BuildBlock = () => new ActivityBlock(func)
            };
        }

        public static Workflow<TInput, TOutput> Activity<TInput, TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new Workflow<TInput, TOutput>
            {
                BuildBlock = () => new ActivityBlock(func)
            };
        }
    }
}