using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public abstract class ParallelDo
    {
        internal Func<IEnumerable<WorkflowBlock>> BuildWorkflows = () => Enumerable.Empty<WorkflowBlock>();
        internal int MaxWorkers = 1;

        internal Func<IEnumerable<WorkflowBlock>> WithBranch(Delegate method)
        {
            return () => BuildWorkflows().Union(new[] {new ActivityBlock(method)});
        }

        internal Func<IEnumerable<WorkflowBlock>> WithBranch<TInput, TOutput>(WorkflowBuilder<TInput, TOutput> workflowBuilder)
        {
            return () => BuildWorkflows().Union(new[] {(workflowBuilder.Build2())});
        }

        internal WorkflowBuilder<TInput, TOutput> Workflow<TInput, TOutput>(Delegate method)
        {
            return new WorkflowBuilder<TInput, TOutput>
            {
                BuildWorkflow = () => new SequenceBlock(new WorkflowBlock[]
                {
                    new ParallelBlock(BuildWorkflows(), MaxWorkers),
                    new ActivityBlock(method)
                })
            };
        }
    }

    public class ParallelDo<TInput> : ParallelDo
    {
        public ParallelDo<TInput, TOutput> Do<TOutput>(Func<TInput, TOutput> func)
        {
            return new ParallelDo<TInput, TOutput>
            {
                BuildWorkflows = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TOutput> Do<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new ParallelDo<TInput, TOutput>
            {
                BuildWorkflows = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TOutput> Do<TOutput>(WorkflowBuilder<TInput, TOutput> workflowBuilder)
        {
            return new ParallelDo<TInput, TOutput>
            {
                BuildWorkflows = WithBranch(workflowBuilder),
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
                BuildWorkflows = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TOutput> Do<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new ParallelDo<TInput, TResult1, TOutput>
            {
                BuildWorkflows = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TOutput> Do<TOutput>(WorkflowBuilder<TInput, TOutput> workflowBuilder)
        {
            return new ParallelDo<TInput, TResult1, TOutput>
            {
                BuildWorkflows = WithBranch(workflowBuilder),
                MaxWorkers = MaxWorkers
            };
        }

        public WorkflowBuilder<TInput, TOutput> Join<TOutput>(Func<TResult1, TOutput> func)
        {
            return Workflow<TInput, TOutput>(func);
        }
    }

    public class ParallelDo<TInput, TResult1, TResult2> : ParallelDo
    {
        public ParallelDo<TInput, TResult1, TResult2, TOutput> Do<TOutput>(Func<TInput, TOutput> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TOutput>
            {
                BuildWorkflows = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TOutput> Do<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TOutput>
            {
                BuildWorkflows = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TOutput> Do<TOutput>(WorkflowBuilder<TInput, TOutput> workflowBuilder)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TOutput>
            {
                BuildWorkflows = WithBranch(workflowBuilder),
                MaxWorkers = MaxWorkers
            };
        }

        public WorkflowBuilder<TInput, TOutput> Join<TOutput>(Func<TResult1, TResult2, TOutput> func)
        {
            return Workflow<TInput, TOutput>(func);
        }
    }

    public class ParallelDo<TInput, TResult1, TResult2, TResult3> : ParallelDo
    {
        public ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput> Do<TOutput>(Func<TInput, TOutput> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput>
            {
                BuildWorkflows = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput> Do<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput>
            {
                BuildWorkflows = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput> Do<TOutput>(WorkflowBuilder<TInput, TOutput> workflowBuilder)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TOutput>
            {
                BuildWorkflows = WithBranch(workflowBuilder),
                MaxWorkers = MaxWorkers
            };
        }

        public WorkflowBuilder<TInput, TOutput> Join<TOutput>(Func<TResult1, TResult2, TResult3, TOutput> func)
        {
            return Workflow<TInput, TOutput>(func);
        }
    }

    public class ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4> : ParallelDo
    {
        public ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput> Do<TOutput>(
            Func<TInput, TOutput> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput>
            {
                BuildWorkflows = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput> Do<TOutput>(
            Func<TInput, Task<TOutput>> func)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput>
            {
                BuildWorkflows = WithBranch(func),
                MaxWorkers = MaxWorkers
            };
        }

        public ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput> Do<TOutput>(
            WorkflowBuilder<TInput, TOutput> workflowBuilder)
        {
            return new ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TOutput>
            {
                BuildWorkflows = WithBranch(workflowBuilder),
                MaxWorkers = MaxWorkers
            };
        }

        public WorkflowBuilder<TInput, TOutput> Join<TOutput>(Func<TResult1, TResult2, TResult3, TResult4, TOutput> func)
        {
            return Workflow<TInput, TOutput>(func);
        }
    }

    public class ParallelDo<TInput, TResult1, TResult2, TResult3, TResult4, TResult5> : ParallelDo
    {
        public WorkflowBuilder<TInput, TOutput> Join<TOutput>(
            Func<TResult1, TResult2, TResult3, TResult4, TResult5, TOutput> func)
        {
            return Workflow<TInput, TOutput>(func);
        }
    }
}