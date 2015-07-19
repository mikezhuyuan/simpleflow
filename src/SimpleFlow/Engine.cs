using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SimpleFlow.Core
{
    interface IEngine
    {
        Task Completion { get; }
        void Kick(WorkItem workItem);
        void Rescure(int workItemId);
        void PostActivity(int workItemId);
    }

    class Engine : IEngine
    {
        readonly IActivityRunner _activityRunner;
        readonly IWorkItemRepository _repository;
        readonly IStateMachineProvider _stateMachineProvider;
        readonly ActionBlock<int> _stateQueue;
        readonly ActionBlock<int> _workerQueue;

        public Engine(IWorkItemRepository repository, IActivityRunner activityRunner,
            IStateMachineProvider stateMachineProvider)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (activityRunner == null) throw new ArgumentNullException("activityRunner");
            if (stateMachineProvider == null) throw new ArgumentNullException("stateMachineProvider");

            _repository = repository;
            _activityRunner = activityRunner;
            _stateMachineProvider = stateMachineProvider;

            _stateQueue = new ActionBlock<int>(id => UpdateState(id),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 1
                });

            _workerQueue = new ActionBlock<int>(id => RunActivity(id),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = int.MaxValue
                });

            _stateQueue.Completion.ContinueWith(t => { _workerQueue.Complete(); }, TaskContinuationOptions.OnlyOnFaulted);

            _workerQueue.Completion.ContinueWith(t => { ((IDataflowBlock)_stateQueue).Fault(t.Exception); },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Kick(WorkItem workItem)
        {
            if (workItem == null)
            {
                _stateQueue.Complete();
                _workerQueue.Complete();

                return;
            }

            _stateQueue.Post(workItem.Id);
        }

        public void Rescure(int workItemId)
        {
            //todo: if status is failed, publish event to highest priority queue and process it immediately.
            _stateQueue.Post(workItemId);
        }

        public void PostActivity(int workItemId)
        {
            _workerQueue.Post(workItemId);
        }

        public Task Completion
        {
            get { return _stateQueue.Completion; }
        }

        void UpdateState(int workItemId)
        {
            var workItem = _repository.Get(workItemId);

            var machine = _stateMachineProvider.Resolve(workItem);

            machine.Transit(workItem, this);
        }

        async Task RunActivity(int workItemId)
        {
            var workItem = _repository.Get(workItemId);

            await _activityRunner.Run(workItem);

            if (workItem.Status == WorkItemStatus.Failed)
            {
                Rescure(workItem.Id);
            }
            else
            {
                var parent = _repository.GetParent(workItem);

                Kick(parent);
            }
        }
    }
}