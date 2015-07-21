using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public class Fork
    {
        internal int MaxWorkers;

        public Workflow<IEnumerable<TInput>, IEnumerable<TOutput>> ForEach<TInput, TOutput>(Func<TInput, TOutput> func)
        {
            return new Workflow<IEnumerable<TInput>, IEnumerable<TOutput>>
            {
                BuildBlock = () => new ForkBlock(new ActivityBlock(func), MaxWorkers)
            };
        }

        public Workflow<IEnumerable<TInput>, IEnumerable<TOutput>> ForEach<TInput, TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new Workflow<IEnumerable<TInput>, IEnumerable<TOutput>>
            {
                BuildBlock = () => new ForkBlock(new ActivityBlock(func), MaxWorkers)
            };
        }

        public Workflow<IEnumerable<TInput>, IEnumerable<TOutput>> ForEach<TInput, TOutput>(Workflow<TInput, TOutput> workflow)
        {
            return new Workflow<IEnumerable<TInput>, IEnumerable<TOutput>>
            {
                BuildBlock = () => new ForkBlock(workflow.BuildBlock(), MaxWorkers)
            };
        }
    }
}