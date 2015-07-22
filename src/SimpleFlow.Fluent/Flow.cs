using System;
using System.Threading.Tasks;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public static class Flow
    {
        public static SequenceBegin Sequence()
        {
            return new SequenceBegin();
        }

        public static ParallelDo Parallel(int maxWorkers = 1)
        {
            return new ParallelDo
            {
                MaxWorkers = maxWorkers
            };
        }

        public static Fork Fork(int maxWorkers = 1)
        {
            return new Fork
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