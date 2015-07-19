using System;
using System.Linq;

namespace SimpleFlow.Core
{
    class SequenceStateMachine : IStateMachine
    {
        readonly IWorkItemRepository _repository;
        readonly IWorkItemBuilder _workItemBuilder;

        public SequenceStateMachine(IWorkItemRepository repository, IWorkItemBuilder workItemBuilder)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (workItemBuilder == null) throw new ArgumentNullException("workItemBuilder");

            _repository = repository;
            _workItemBuilder = workItemBuilder;
        }

        public void Transit(WorkItem workItem, IEngine engine)
        {
            if (workItem == null) throw new ArgumentNullException("workItem");
            if (workItem.Type != WorkflowType.Sequence) throw new ArgumentException("type must be sequence");

            var status = workItem.Status;

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

                    if (inProgress == 0)
                    {
                        var next = _repository.FindRunnableChildrenByOrder(workItem.Id, 1).SingleOrDefault();

                        if (next == null) // all complete
                        {
                            var last = _repository.GetLastChildByOrder(workItem.Id);

                            workItem.OutputId = last.OutputId;

                            workItem.Status = WorkItemStatus.Completed;
                            _repository.Update(workItem);

                            engine.Kick(_repository.GetParent(workItem));
                        }
                        else
                        {
                            if (next.Order > 0)
                            {
                                var previous = _repository.GetChildByOrder(workItem.Id, next.Order - 1);
                                next.InputId = previous.OutputId;
                                _repository.Update(next);
                            }

                            engine.Kick(next);
                        }
                    }

                    break;
            }
        }
    }
}