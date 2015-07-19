using System;

namespace SimpleFlow.Core
{
    interface IStateMachineProvider
    {
        IStateMachine Resolve(WorkItem workItem);
    }

    class StateMachineProvider : IStateMachineProvider
    {
        readonly IDataStore _dataStore;
        readonly IWorkItemRepository _repository;
        readonly IWorkflowPathNavigator _workflowPathNavigator;
        readonly IWorkItemBuilder _workItemBuilder;

        public StateMachineProvider(
            IWorkItemRepository repository,
            IWorkItemBuilder workItemBuilder,
            IWorkflowPathNavigator workflowPathNavigator,
            IDataStore dataStore)
        {
            _repository = repository;
            _workItemBuilder = workItemBuilder;
            _workflowPathNavigator = workflowPathNavigator;
            _dataStore = dataStore;
        }

        public IStateMachine Resolve(WorkItem workItem)
        {
            if (workItem == null) throw new ArgumentNullException("workItem");

            if(workItem.Status == WorkItemStatus.Failed)
                return new ExceptionStateMachine(_repository, _dataStore, _workflowPathNavigator);

            switch (workItem.Type)
            {
                case WorkflowType.Activity:
                    return new ActivityStateMachine(_repository);
                case WorkflowType.Fork:
                    return new ForkStateMachine(_repository, _workItemBuilder, _workflowPathNavigator, _dataStore);
                case WorkflowType.Parallel:
                    return new ParallelStateMachine(_repository, _workItemBuilder, _workflowPathNavigator, _dataStore);
                case WorkflowType.Sequence:
                    return new SequenceStateMachine(_repository, _workItemBuilder);
            }

            throw new ArgumentException("workItem type does not support: " + workItem.Type);
        }
    }
}