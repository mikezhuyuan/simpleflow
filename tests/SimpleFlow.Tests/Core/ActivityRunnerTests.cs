using System;
using System.Threading.Tasks;
using NSubstitute;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class ActivityRunnerTests
    {
        readonly ActivityRunner _activityRunner;
        readonly IDataStore _dataStore;
        readonly IWorkItemRepository _repository;
        readonly IWorkflowPathNavigator _workflowPathNavigator;

        public ActivityRunnerTests()
        {
            _dataStore = Substitute.For<IDataStore>();
            _repository = Substitute.For<IWorkItemRepository>();
            _workflowPathNavigator = Substitute.For<IWorkflowPathNavigator>();

            _activityRunner = new ActivityRunner(_workflowPathNavigator, _dataStore, _repository);
        }

        [Fact]
        public async Task MethodWithNoArgumentAndReturnValue()
        {
            var invoked = false;
            Action action = delegate { invoked = true; };
            var workItem = new WorkItem();
            var activity = new ActivityBlock(action);

            await _activityRunner.RunCore(workItem, activity);
            Assert.True(invoked);
        }

        [Fact]
        public async Task IgnoringInput()
        {
            var invoked = false;
            Action action = delegate { invoked = true; };
            var workItem = new WorkItem {InputId = Helpers.Integer()};
            var activity = new ActivityBlock(action);

            await _activityRunner.RunCore(workItem, activity);
            Assert.True(invoked);
        }

        [Fact]
        public async Task MethodWithReturnValue()
        {
            Func<int> func = () => 1;
            var workItem = new WorkItem {JobId = Helpers.Integer()};
            var activity = new ActivityBlock(func);

            await _activityRunner.RunCore(workItem, activity);

            _dataStore.Received(1).Add(workItem.JobId, 1, typeof (int));
        }

        [Fact]
        public async Task MethodWithSingleArgument()
        {
            Func<int, int> identity = i => i;
            var workItem = new WorkItem {JobId = Helpers.Integer(), InputId = Helpers.Integer()};
            var activity = new ActivityBlock(identity);

            _dataStore.Get(workItem.InputId.Value).Returns(2);

            await _activityRunner.RunCore(workItem, activity);

            _dataStore.Received(1).Add(workItem.JobId, 2, typeof (int));
        }

        [Fact]
        public async Task MethodWithMultipleArguments()
        {
            Func<int, int, int> add = (x, y) => x + y;
            var workItem = new WorkItem {JobId = Helpers.Integer(), InputId = Helpers.Integer()};
            var activity = new ActivityBlock(add);

            _dataStore.Get(workItem.InputId.Value).Returns(new object[] {1, 2});

            await _activityRunner.RunCore(workItem, activity);

            _dataStore.Received(1).Add(workItem.JobId, 3, typeof (int));
        }

        [Fact]
        public async Task TestAsyncMethod()
        {
            Func<Task<int>> oneInFuture = () => Task.FromResult(1);

            var workItem = new WorkItem();
            var activity = new ActivityBlock(oneInFuture);

            await _activityRunner.RunCore(workItem, activity);

            _dataStore.Received(1).Add(workItem.JobId, 1, typeof (int));
        }
    }
}