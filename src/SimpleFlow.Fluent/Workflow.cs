using System;
using System.Threading.Tasks;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public interface IWorkflowBuilder //todo: integrate workflowdefinition
    {
        WorkflowBlock Build2(); //renmaing
    }

    public class WorkflowBuilder<TInput, TOutput> : IWorkflowBuilder
    {
        internal Func<WorkflowBlock> BuildWorkflow { get; set; }

        public WorkflowBlock Build2()
        {
           return BuildWorkflow();
        }

        public Workflow<TInput, TOutput> Build(string name)
        {
            var definition = BuildWorkflow();

            return new Workflow<TInput, TOutput>(new WorkflowDefinition(name, definition));
        }
    }

    public class Workflow<TInput, TOutput>
    {
        readonly WorkflowDefinition _definition;

        public Workflow(WorkflowDefinition definition)
        {
            _definition = definition;
        }

        public Task<TOutput> Start(TInput input)
        {
            return Workflow.Start<TInput, TOutput>(input, _definition);
        }
    }
}