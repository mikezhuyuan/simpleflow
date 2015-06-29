using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFlow.Core
{
    interface IActivityRunner
    {
        Task Run(WorkItem workItem);
    }

    class ActivityRunner : IActivityRunner
    {
        readonly IDataStore _dataStore;
        readonly IWorkItemRepository _repository;
        readonly IWorkflowPathNavigator _workflowPathNavigator;

        public ActivityRunner(IWorkflowPathNavigator workflowPathNavigator, IDataStore dataStore,
            IWorkItemRepository repository)
        {
            if (workflowPathNavigator == null) throw new ArgumentNullException("workflowPathNavigator");
            if (dataStore == null) throw new ArgumentNullException("dataStore");
            if (repository == null) throw new ArgumentNullException("repository");

            _workflowPathNavigator = workflowPathNavigator;
            _dataStore = dataStore;
            _repository = repository;
        }

        public Task Run(WorkItem workItem)
        {
            if (workItem == null) throw new ArgumentNullException("workItem");
            if (workItem.Type != WorkflowType.Activity) throw new ArgumentException("type must be Activity");

            var definition = _workflowPathNavigator.Find(workItem.WorkflowPath);

            if (definition == null)
                throw new Exception("definition not found with path: " + workItem.WorkflowPath);
            //todo: specific exception
            if (!(definition is ActivityBlock))
                throw new Exception("definition is expected to be ActivityBlock but actually received: " +
                                    definition.Type); //todo: specific excpetion

            var activity = (ActivityBlock) definition;

            return RunCore(workItem, activity);
        }

        internal async Task RunCore(WorkItem workItem, ActivityBlock activityBlock)
        {
            workItem.Status = WorkItemStatus.Running;
            _repository.Update(workItem);

            object output;
            var inputTypes = activityBlock.InputTypes.ToArray();

            if (inputTypes.Length == 0)
            {
                if (activityBlock.IsAsync)
                {
                    var task = (Task) activityBlock.Method.DynamicInvoke();
                    await task;

                    output = task.GetResult();
                }
                else
                {
                    output = activityBlock.Method.DynamicInvoke();
                }
            }
            else if (inputTypes.Length == 1)
            {
                var input = _dataStore.Get(workItem.InputId.Value);

                if (activityBlock.IsAsync)
                {
                    var task = (Task) activityBlock.Method.DynamicInvoke(input);
                    await task;

                    output = task.GetResult();
                }
                else
                {
                    output = activityBlock.Method.DynamicInvoke(input);
                }
            }
            else
            {
                var args = (object[]) _dataStore.Get(workItem.InputId.Value);

                if (activityBlock.IsAsync)
                {
                    var task = (Task) activityBlock.Method.DynamicInvoke(args);
                    await task;

                    output = task.GetResult();
                }
                else
                {
                    output = activityBlock.Method.DynamicInvoke(args);
                }
            }

            if (output != null)
            {
                workItem.OutputId = _dataStore.Add(workItem.JobId, output, activityBlock.OutputType);
            }
        }
    }
}