using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public class SequenceThen<TInitialInput, TInput>
    {
        internal Func<IEnumerable<WorkflowBlock>> BuildWorkflows = () => Enumerable.Empty<WorkflowBlock>();

        public SequenceThen<TInitialInput, TOutput> Then<TOutput>(Func<TInput, TOutput> func)
        {
            return new SequenceThen<TInitialInput, TOutput>
            {
                BuildWorkflows = () => BuildWorkflows().Union(new[] { new ActivityBlock(func) })
            };
        }

        public SequenceThen<TInitialInput, TOutput> Then<TOutput>(Func<TInput, Task<TOutput>> func)
        {
            return new SequenceThen<TInitialInput, TOutput>
            {
                BuildWorkflows = () => BuildWorkflows().Union(new[] { new ActivityBlock(func) })
            };
        }

        public SequenceThen<TInitialInput, IEnumerable<TOutput>> Then<TOutput>(Func<TInput, TOutput[]> func) //what's the better way of not having this override
        {
            return new SequenceThen<TInitialInput, IEnumerable<TOutput>>
            {
                BuildWorkflows = () => BuildWorkflows().Union(new[] { new ActivityBlock(func) })
            };
        }

        public SequenceThen<TInitialInput, TOutput> Then<TOutput>(WorkflowBuilder<TInput, TOutput> workflowBuilder)
        {
            return new SequenceThen<TInitialInput, TOutput>
            {
                BuildWorkflows = () => BuildWorkflows().Union(new[] { workflowBuilder.BuildWorkflow() })
            };
        }

        public SequenceThen<TInitialInput, IEnumerable<TOutput>> Then<TOutput>(WorkflowBuilder<TInput, TOutput[]> workflowBuilder)
        {
            return new SequenceThen<TInitialInput, IEnumerable<TOutput>>
            {
                BuildWorkflows = () => BuildWorkflows().Union(new[] { workflowBuilder.BuildWorkflow() })
            };
        }

        public WorkflowBuilder<TInitialInput, TInput> End()
        {
            return new WorkflowBuilder<TInitialInput, TInput>
            {
                BuildWorkflow = () => new SequenceBlock(BuildWorkflows())
            };
        }
    }
}