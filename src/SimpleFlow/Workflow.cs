using System.Threading.Tasks;

namespace SimpleFlow.Core
{
    public static class Workflow
    {
        public static Task<TOutput> Start<TInput, TOutput>(TInput input, WorkflowDefinition definition)
        {
            IDataStore dataStore = new InMemoryDataStore();
            IWorkItemRepository workItemRepo = new InMemoryWorkItemRepository();
            IWorkflowPathNavigator pathNavigator = new WorkflowPathNavigator(definition);
            IActivityRunner activityRunner = new ActivityRunner(pathNavigator, dataStore, workItemRepo);
            IWorkItemBuilder workItemBuilder = new WorkItemBuilder(pathNavigator, dataStore);
            IStateMachineProvider stateMachineProvider = new StateMachineProvider(workItemRepo, workItemBuilder,
                pathNavigator, dataStore);
            IEngine engine = new Engine(workItemRepo, activityRunner, stateMachineProvider);

            var jobId = 1; //todo: create job
            var inputId = dataStore.Add(jobId, input, typeof (TInput));
            var root = new WorkItem(jobId, null, 0, definition.Root.Type, pathNavigator.Path(definition.Root))
            {
                InputId = inputId
            };

            var rootId = workItemRepo.Add(root);

            engine.Kick(root);

            return engine.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    throw t.Exception.Flatten();

                var wi = workItemRepo.Get(rootId);
                return dataStore.Get<TOutput>(wi.OutputId.Value);
            });
        }
    }
}