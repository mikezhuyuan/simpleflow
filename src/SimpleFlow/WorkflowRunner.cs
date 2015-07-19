using System;
using System.Threading.Tasks;

namespace SimpleFlow.Core
{
    public static class WorkflowRunner // todo: configurable and transient service
    {
        public static Task<TOutput> Run<TInput, TOutput>(Workflow<TInput, TOutput> workflow, TInput input)
            //todo: use container
        {
            IDataStore dataStore = new InMemoryDataStore();
            IWorkItemRepository workItemRepo = new InMemoryWorkItemRepository();
            IWorkflowPathNavigator pathNavigator = new WorkflowPathNavigator(workflow.Root);
            IActivityRunner activityRunner = new ActivityRunner(pathNavigator, dataStore, workItemRepo);
            IWorkItemBuilder workItemBuilder = new WorkItemBuilder(pathNavigator, dataStore);
            IStateMachineProvider stateMachineProvider = new StateMachineProvider(workItemRepo, workItemBuilder,
                pathNavigator, dataStore);
            IEngine engine = new Engine(workItemRepo, activityRunner, stateMachineProvider);

            var jobId = 1; //todo: create job
            var inputId = dataStore.Add(jobId, input, typeof (TInput));
            var root = new WorkItem(jobId, null, 0, workflow.Root.Type, pathNavigator.Path(workflow.Root))
            {
                InputId = inputId
            };

            var rootId = workItemRepo.Add(root);

            engine.Kick(root.Id);

            return engine.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    throw t.Exception.Flatten();

                var wi = workItemRepo.Get(rootId);
                if (wi.Status == WorkItemStatus.Failed)
                {
                    var ex = (Exception) dataStore.Get(wi.ExceptionId.Value);
                    throw ex;
                }

                return dataStore.Get<TOutput>(wi.OutputId.Value);
            });
        }
    }
}