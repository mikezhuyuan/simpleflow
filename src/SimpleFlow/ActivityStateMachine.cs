namespace SimpleFlow.Core
{
    //todo: add unit tests
    class ActivityStateMachine : IStateMachine
    {
        readonly IWorkItemRepository _repository;

        public ActivityStateMachine(IWorkItemRepository repository)
        {
            _repository = repository;
        }

        public void Transit(WorkItem workItem, IEngine engine)
        {
            switch (workItem.Status)
            {
                case WorkItemStatus.Created:
                    workItem.Status = WorkItemStatus.Pending;
                    _repository.Update(workItem);

                    engine.PostActivity(workItem.Id);
                    break;
            }
        }
    }
}