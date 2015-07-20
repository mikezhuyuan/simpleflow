using System;
using NSubstitute;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class ExceptionStateMachineTests
    {
        readonly IDataStore _dataStore;
        readonly IEngine _engine;
        readonly IWorkflowPathNavigator _navigator;
        readonly ExceptionStateMachine _stateMachine;
        readonly InMemoryWorkItemRepository _workItemRepo;

        public ExceptionStateMachineTests()
        {
            _dataStore = Substitute.For<IDataStore>();
            _workItemRepo = new InMemoryWorkItemRepository();
            _navigator = Substitute.For<IWorkflowPathNavigator>();
            _engine = Substitute.For<IEngine>();
            _stateMachine = new ExceptionStateMachine(_workItemRepo, _dataStore, _navigator);
        }

        [Fact]
        public void CanRescure()
        {
            var parent = new WorkItem
            {
                JobId = Helpers.Integer(),
                WorkflowPath = "parent"
            };

            var parentId = _workItemRepo.Add(parent);

            var child = new WorkItem
            {
                ParentId = parentId,
                WorkflowPath = "child",
                Status = WorkItemStatus.Failed,
                ExceptionId = Helpers.Integer()
            };

            _workItemRepo.Add(child);

            var definition = Helpers.BuildFork();
            definition.ExceptionHandler = (Func<Exception, int>) (_ => 1);

            _navigator.Find(parent.WorkflowPath).Returns(definition);

            _dataStore.Add(parent.JobId, 1, typeof (int)).ReturnsForAnyArgs(1);
            _stateMachine.Transit(child, _engine);

            parent = _workItemRepo.Get(parentId);

            Assert.Equal(WorkItemStatus.Completed, parent.Status);
            Assert.Equal(1, parent.OutputId);

            _engine.Kick(parentId);
        }

        [Fact]
        public void BubblesUpIfHandlerHasNotDefined()
        {
            var root = new WorkItem
            {
                JobId = Helpers.Integer(),
                WorkflowPath = "root"
            };

            _workItemRepo.Add(root);

            var parent = new WorkItem
            {
                ParentId = root.Id,
                WorkflowPath = "parent"
            };

            _workItemRepo.Add(parent);

            var child = new WorkItem
            {
                ExceptionId = Helpers.Integer(),
                ParentId = parent.Id,
                WorkflowPath = "child",
                Status = WorkItemStatus.Failed
            };

            _workItemRepo.Add(child);

            _navigator.Find(root.WorkflowPath).Returns(Helpers.BuildFork());
            _navigator.Find(parent.WorkflowPath).Returns(Helpers.BuildFork());

            _stateMachine.Transit(child, _engine);

            var wi = _workItemRepo.Get(parent.Id);
            Assert.Equal(WorkItemStatus.Failed, wi.Status);
            Assert.Equal(child.ExceptionId, wi.ExceptionId);

            wi = _workItemRepo.Get(root.Id);
            Assert.Equal(WorkItemStatus.Failed, wi.Status);
            Assert.Equal(child.ExceptionId, wi.ExceptionId);
            _engine.Kick(null);
        }

        [Theory]
        [InlineData(WorkItemStatus.Failed)]
        [InlineData(WorkItemStatus.Completed)]
        public void ShouldNotContinueIfParentIsFinalState(WorkItemStatus state)
        {
            var parent = new WorkItem
            {
                JobId = Helpers.Integer(),
                WorkflowPath = "parent",
                Status = state
            };

            var parentId = _workItemRepo.Add(parent);

            var child = new WorkItem
            {
                ParentId = parentId,
                WorkflowPath = "child",
                Status = WorkItemStatus.Failed,
                ExceptionId = Helpers.Integer()
            };

            _navigator.Find(parent.WorkflowPath).Returns(Helpers.BuildFork());

            _dataStore.Add(parent.JobId, 1, typeof (int)).ReturnsForAnyArgs(1);
            _stateMachine.Transit(child, _engine);

            _engine.DidNotReceiveWithAnyArgs().Kick(null);
        }
    }
}