using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public abstract class ParallelDo
    {
        internal Func<IEnumerable<WorkflowBlock>> BuildBlocks = () => Enumerable.Empty<WorkflowBlock>();
        internal int MaxWorkers = 1;

        internal Func<IEnumerable<WorkflowBlock>> WithBranch(Delegate method)
        {
            return () => BuildBlocks().Union(new[] {new ActivityBlock(method)});
        }

        internal Func<IEnumerable<WorkflowBlock>> WithBranch<TInput, TOutput>(Workflow<TInput, TOutput> workflow)
        {
            return () => BuildBlocks().Union(new[] {(workflow.BuildBlock())});
        }

        internal Workflow<TInput, TOutput> Workflow<TInput, TOutput>()
        {
            return new Workflow<TInput, TOutput>
            {
                BuildBlock = () => new ParallelBlock(BuildBlocks(), MaxWorkers)
            };
        }
    }

    public class ParallelDo<TInput> : ParallelDo
    {
        public ParallelDo<TInput, TOutput> Do<TOutput>(Func<TInput, TOutput> func)
        {
            return new ParallelDo<TInput, TOutput>
            {
                BuildBlocks = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TOutput> Do<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new ParallelDo<TInput, TOutput>
            {
                BuildBlocks = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TOutput> Do<TOutput>(Workflow<TInput, TOutput> workflow)
        {
            return new ParallelDo<TInput, TOutput>
            {
                BuildBlocks = WithBranch(workflow),
                MaxWorkers = MaxWorkers
            };
        }
    }

    public class ParallelDo<TInput, TResult1> : ParallelDo
    {
        public ParallelDo<TInput, TResult1, TOutput> Do<TOutput>(Func<TInput, TOutput> func)
        {
            return new ParallelDo<TInput, TResult1, TOutput>
            {
                BuildBlocks = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TOutput> Do<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new ParallelDo<TInput, TResult1, TOutput>
            {
                BuildBlocks = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TOutput> Do<TOutput>(Workflow<TInput, TOutput> workflow)
        {
            return new ParallelDo<TInput, TResult1, TOutput>
            {
                BuildBlocks = WithBranch(workflow),
                MaxWorkers = MaxWorkers
            };
        }
    }

    public class ParallelDo<TInput, TResult1, TResult2> : ParallelDo
    {
        public ParallelDo<TInput, TResult1, TResult2, TOutput> Do<TOutput>(Func<TInput, TOutput> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TOutput>
            {
                BuildBlocks = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TOutput> Do<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TOutput>
            {
                BuildBlocks = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TOutput> Do<TOutput>(Workflow<TInput, TOutput> workflow)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TOutput>
            {
                BuildBlocks = WithBranch(workflow),
                MaxWorkers = MaxWorkers
            };
        }

        public Workflow<TInput, Tuple<TResult1, TResult2>> Join()
        {
            return Workflow<TInput, Tuple<TResult1, TResult2>>();
        }
    }

    public class ParallelDo<TInput, TResult1, TResult2, TResult3> : ParallelDo
    {
        public ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput> Do<TOutput>(Func<TInput, TOutput> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput>
            {
                BuildBlocks = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput> Do<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput>
            {
                BuildBlocks = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput> Do<TOutput>(Workflow<TInput, TOutput> workflow)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput>
            {
                BuildBlocks = WithBranch(workflow),
                MaxWorkers = MaxWorkers
            };
        }

        public Workflow<TInput, Tuple<TResult1, TResult2, TResult3>> Join()
        {
            return Workflow<TInput, Tuple<TResult1, TResult2, TResult3>>();
        }
    }

    public class ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4> : ParallelDo
    {
        public ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput> Do<TOutput>(
            Func<TInput, TOutput> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput>
            {
                BuildBlocks = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput> Do<TOutput>(
            Func<TInput, Task<TOutput>> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput>
            {
                BuildBlocks = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput> Do<TOutput>(
            Workflow<TInput, TOutput> workflow)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput>
            {
                BuildBlocks = WithBranch(workflow),
                MaxWorkers = MaxWorkers
            };
        }

        public Workflow<TInput, Tuple<TResult1, TResult2, TResult3, TResult4>> Join()
        {
            return Workflow<TInput, Tuple<TResult1, TResult2, TResult3, TResult4>>();
        }
    }

    public class ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TResult5> : ParallelDo
    {
        public Workflow<TInput, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5>> Join()
        {
            return Workflow<TInput, Tuple<TResult1, TResult2, TResult3, TResult4, TResult5>>();
        }
    }
}