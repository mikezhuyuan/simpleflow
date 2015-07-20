namespace SimpleFlow.Core
{
    //move to specific class, does't implement IStateMachine
    class ExceptionStateMachine: IStateMachine
    {
        readonly IWorkItemRepository _repository;
        readonly IDataStore _dataStore;
        readonly IWorkflowPathNavigator _navigator;

        public ExceptionStateMachine(IWorkItemRepository repository, IDataStore dataStore, IWorkflowPathNavigator navigator)
        {
            _repository = repository;
            _dataStore = dataStore;
            _navigator = navigator;
        }

        public void Transit(WorkItem workItem, IEngine engine)
        {
            var exceptionId = workItem.ExceptionId.Value;
            var exception = _dataStore.Get(exceptionId);
            var current = workItem;
            current = _repository.GetParent(current);

            while (current != null)
            {
                if (current.Status.IsFinal())
                    return;

                if (current.Type == WorkflowType.Retry) //todo: add unit test
                {
                    engine.Kick(current.Id);
                    return;
                }

                if (TryRescure(current, exception, exceptionId))
                    break;

                current = _repository.GetParent(current);
            }

            if (current == null)
            {
                engine.Kick(null);
                return;
            }

            engine.Kick(current.ParentId);
        }

        bool TryRescure(WorkItem workItem, object exception, int exceptionId)
        {
            var definition = _navigator.Find(workItem.WorkflowPath);
            workItem.ExceptionId = exceptionId;

            if (definition.ExceptionHandler == null)
            {
                workItem.Status = WorkItemStatus.Failed;
                _repository.Update(workItem);
                return false;
            }

            var output = definition.ExceptionHandler.DynamicInvoke(exception);

            workItem.OutputId = _dataStore.Add(workItem.JobId, output, definition.OutputType);
            workItem.Status = WorkItemStatus.Completed;
            _repository.Update(workItem);

            return true;
        }
    }
}