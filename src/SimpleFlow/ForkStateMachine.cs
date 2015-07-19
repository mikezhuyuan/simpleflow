using System;

namespace SimpleFlow.Core
{
    class ForkStateMachine : IStateMachine
    {
        readonly IDataStore _dataStore;
        readonly IWorkItemRepository _repository;
        readonly IWorkflowPathNavigator _workflowPathNavigator;
        readonly IWorkItemBuilder _workItemBuilder;

        public ForkStateMachine(IWorkItemRepository repository, IWorkItemBuilder workItemBuilder,
            IWorkflowPathNavigator workflowPathNavigator,
            IDataStore dataStore)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (workflowPathNavigator == null) throw new ArgumentNullException("workflowPathNavigator");
            if (workItemBuilder == null) throw new ArgumentNullException("workItemBuilder");
            if (dataStore == null) throw new ArgumentNullException("dataStore");

            _repository = repository;
            _workflowPathNavigator = workflowPathNavigator;
            _workItemBuilder = workItemBuilder;
            _dataStore = dataStore;
        }

        public void Transit(WorkItem workItem, IEngine engine)
        {
            if (workItem == null) throw new ArgumentNullException("workItem");
            if (workItem.Type != WorkflowType.Fork) throw new ArgumentException("type must be fork");

            var status = workItem.Status;
            var definition = (ForkBlock) _workflowPathNavigator.Find(workItem.WorkflowPath);

            switch (status)
            {
                case WorkItemStatus.Created:
                    _repository.DeleteChildren(workItem.Id);

                    var children = _workItemBuilder.BuildChildren(workItem);

                    _repository.AddAll(children);

                    workItem.Status = WorkItemStatus.WaitingForChildren;
                    _repository.Update(workItem);

                    engine.Kick(workItem);

                    break;
                case WorkItemStatus.WaitingForChildren:
                    if (_repository.HasFailedChildren(workItem.Id))
                        return;

                    var inProgress = _repository.CountInProgressChildren(workItem.Id);
                    var newWorkers = definition.MaxWorkers - inProgress;

                    if (newWorkers > 0)
                    {
                        var workItems = _repository.FindRunnableChildrenByOrder(workItem.Id, newWorkers);
                        if (workItems.Count > 0)
                        {
                            foreach (var runnable in workItems)
                            {
                                engine.Kick(runnable);
                            }
                        }
                        else if (inProgress == 0)
                        {
                            var ids = _repository.LoadChildOutputIds(workItem.Id);
                            workItem.OutputId = _dataStore.AddReferences(workItem.JobId, ids,
                                definition.Child.OutputType);
                            workItem.Status = WorkItemStatus.Completed;
                            _repository.Update(workItem);

                            engine.Kick(_repository.GetParent(workItem));
                        }
                    }

                    break;
            }
        }
    }
}