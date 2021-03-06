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

        public async Task Run(WorkItem workItem)
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

            try
            {
                await RunCore(workItem, activity);
                workItem.Status = WorkItemStatus.Completed;
            }
            catch (Exception ex)
            {
                // todo: Here exception is thrown from dynamic invoke, the actual failure is inside InnerException, is that safe to lose context?
                // todo: log original exception
                var actualException = ex.InnerException ?? ex;

                // todo: consider combine it with engine
                if (definition.ExceptionHandler == null)
                {
                    workItem.ExceptionId = _dataStore.Add(workItem.JobId, actualException, actualException.GetType());
                    workItem.Status = WorkItemStatus.Failed;
                }
                else
                {
                    var output = definition.ExceptionHandler.DynamicInvoke(actualException);

                    workItem.OutputId = _dataStore.Add(workItem.JobId, output, activity.OutputType);
                    workItem.Status = WorkItemStatus.Completed;
                }
            }

            _repository.Update(workItem);
        }

        async Task RunCore(WorkItem workItem, ActivityBlock activityBlock)
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
                var arg = _dataStore.Get(workItem.InputId.Value);

                if (inputTypes[0].IsTuple() && arg is object[])
                    // If output is array from parallel and input of continuation is Tuple
                {
                    var ctor = inputTypes[0].GetConstructors().Single();
                    arg = ctor.Invoke((object[]) arg);
                }

                if (activityBlock.IsAsync)
                {
                    var task = (Task) activityBlock.Method.DynamicInvoke(arg);
                    await task;

                    output = task.GetResult();
                }
                else
                {
                    output = activityBlock.Method.DynamicInvoke(arg);
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