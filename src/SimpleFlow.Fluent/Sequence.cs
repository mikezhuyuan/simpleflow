using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public class SequenceBegin
    {
        public SequenceThen<TInput, TOutput> Begin<TInput, TOutput>(Func<TInput, TOutput> func)
        {
            return new SequenceThen<TInput, TOutput>
            {
                BuildBlocks = () => new[] { new ActivityBlock(func) }
            };
        }

        public SequenceThen<TInput, TOutput> Begin<TInput, TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new SequenceThen<TInput, TOutput>
            {
                BuildBlocks = () => new[] { new ActivityBlock(func) }
            };
        }

        public SequenceThen<TInput, IEnumerable<TOutput>> Begin<TInput, TOutput>(Func<TInput, TOutput[]> func)
        {
            return new SequenceThen<TInput, IEnumerable<TOutput>>
            {
                BuildBlocks = () => new[] { new ActivityBlock(func) }
            };
        }

        public SequenceThen<TInput, IEnumerable<TOutput>> Begin<TInput, TOutput>(Func<TInput, Task<TOutput[]>> func)
        {
            return new SequenceThen<TInput, IEnumerable<TOutput>>
            {
                BuildBlocks = () => new[] { new ActivityBlock(func) }
            };
        }

        public SequenceThen<TInput, TOutput> Begin<TInput, TOutput>(Workflow<TInput, TOutput> workflow)
        {
            return new SequenceThen<TInput, TOutput>
            {
                BuildBlocks = () => new[] { workflow.BuildBlock() }
            };
        }

        public SequenceThen<TInput, IEnumerable<TOutput>> Begin<TInput, TOutput>(Workflow<TInput, TOutput[]> workflow)
        {
            return new SequenceThen<TInput, IEnumerable<TOutput>>
            {
                BuildBlocks = () => new[] { workflow.BuildBlock() }
            };
        }
    }

    public class SequenceThen<TInitialInput, TInput>
    {
        internal Func<IEnumerable<WorkflowBlock>> BuildBlocks;

        public SequenceThen<TInitialInput, TOutput> Then<TOutput>(Func<TInput, TOutput> func)
        {
            return new SequenceThen<TInitialInput, TOutput>
            {
                BuildBlocks = () => BuildBlocks().Union(new[] {new ActivityBlock(func)})
            };
        }

        public SequenceThen<TInitialInput, TOutput> Then<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new SequenceThen<TInitialInput, TOutput>
            {
                BuildBlocks = () => BuildBlocks().Union(new[] {new ActivityBlock(func)})
            };
        }

        public SequenceThen<TInitialInput, IEnumerable<TOutput>> Then<TOutput>(Func<TInput, TOutput[]> func)
        {
            return new SequenceThen<TInitialInput, IEnumerable<TOutput>>
            {
                BuildBlocks = () => BuildBlocks().Union(new[] { new ActivityBlock(func) })
            };
        }

        public SequenceThen<TInitialInput, IEnumerable<TOutput>> Then<TOutput>(Func<TInput, Task<TOutput[]>> func)
        {
            return new SequenceThen<TInitialInput, IEnumerable<TOutput>>
            {
                BuildBlocks = () => BuildBlocks().Union(new[] { new ActivityBlock(func) })
            };
        }

        public SequenceThen<TInitialInput, TOutput> Then<TOutput>(Workflow<TInput, TOutput> workflow)
        {
            return new SequenceThen<TInitialInput, TOutput>
            {
                BuildBlocks = () => BuildBlocks().Union(new[] {workflow.BuildBlock()})
            };
        }

        public SequenceThen<TInitialInput, IEnumerable<TOutput>> Then<TOutput>(Workflow<TInput, TOutput[]> workflow)
        {
            return new SequenceThen<TInitialInput, IEnumerable<TOutput>>
            {
                BuildBlocks = () => BuildBlocks().Union(new[] { workflow.BuildBlock() })
            };
        }

        public Workflow<TInitialInput, TInput> End()
        {
            return new Workflow<TInitialInput, TInput>
            {
                BuildBlock = () => new SequenceBlock(BuildBlocks())
            };
        }
    }
}