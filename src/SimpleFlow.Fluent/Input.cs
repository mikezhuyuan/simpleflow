using System.Collections.Generic;

namespace SimpleFlow.Fluent
{
    public class Input<TInput>
    {
    }

    public static class Extensions
    {
        public static SequenceThen<TInput, TInput> Sequence<TInput>(this Input<TInput> input)
        {
            return new SequenceThen<TInput, TInput>();
        }

        public static ForkForEach<TInput> Fork<TInput>(this Input<IEnumerable<TInput>> input, int maxWorkers = 1)
        {
            return new ForkForEach<TInput>
            {
                MaxWorkers = maxWorkers
            };
        }

        public static ParallelDo<TInput> Parallel<TInput>(this Input<TInput> input, int maxWorkers = 1)
        {
            return new ParallelDo<TInput>
            {
                MaxWorkers = maxWorkers
            };
        }
    }
}