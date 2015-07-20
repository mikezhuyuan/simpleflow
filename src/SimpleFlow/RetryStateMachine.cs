using System;

namespace SimpleFlow.Core
{
    class RetryStateMachine : IStateMachine
    {
        readonly IWorkItemRepository _repository;
        readonly IWorkItemBuilder _workItemBuilder;
        readonly IWorkflowPathNavigator _navigator;

        public RetryStateMachine(IWorkItemRepository repository, IWorkItemBuilder workItemBuilder, IWorkflowPathNavigator navigator)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (workItemBuilder == null) throw new ArgumentNullException("workItemBuilder");

            _repository = repository;
            _workItemBuilder = workItemBuilder;
            _navigator = navigator;
        }

        public void Transit(WorkItem workItem, IEngine engine)
        {
            if (workItem == null) throw new ArgumentNullException("workItem");
            if (workItem.Type != WorkflowType.Retry) throw new ArgumentException("type must be retry");

            var status = workItem.Status;

            switch (status)
            {
                case WorkItemStatus.Created:
                    _repository.DeleteChildren(workItem.Id);

                    var children = _workItemBuilder.BuildChildren(workItem);

                    _repository.AddAll(children);

                    workItem.Status = WorkItemStatus.WaitingForChildren;
                    _repository.Update(workItem);

                    engine.Kick(workItem.Id);
                    break;
                case WorkItemStatus.WaitingForChildren:
                    var last = _repository.GetLastChildByOrder(workItem.Id);
                    var definition = (RetryBlock)_navigator.Find(workItem.WorkflowPath);

                    switch (last.Status)
                    {
                        case WorkItemStatus.Created:
                            engine.Kick(last.Id);
                            break;
                        case WorkItemStatus.Completed:
                            workItem.OutputId = last.OutputId;
                            workItem.Status = WorkItemStatus.Completed;
                            
                            _repository.Update(workItem);

                            engine.Kick(workItem.ParentId);
                            break;
                        case WorkItemStatus.Failed:
                            if (last.Order < definition.RetryCount)
                            {
                                var newItem = last.Retry();

                                _repository.Add(newItem);
                                
                                engine.Kick(newItem.Id);
                            }
                            else
                            {
                                workItem.ExceptionId = last.ExceptionId;
                                workItem.Status = WorkItemStatus.Failed;
                                
                                _repository.Update(workItem);
                                
                                engine.Rescure(workItem.Id);
                            }
                            break;
                    }
                    break;
            }
        }
    }
}