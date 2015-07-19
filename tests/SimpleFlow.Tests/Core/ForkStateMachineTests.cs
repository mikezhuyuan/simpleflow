using System.Linq;
using NSubstitute;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class ForkStateMachineTests
    {
        readonly IDataStore _dataStore;
        readonly IEngine _engine;
        readonly IWorkItemRepository _repository;
        readonly IStateMachine _stateMachine;
        readonly IWorkflowPathNavigator _workflowPathNavigator;
        readonly IWorkItemBuilder _workItemBuilder;

        public ForkStateMachineTests()
        {
            _repository = Substitute.For<IWorkItemRepository>();
            _workItemBuilder = Substitute.For<IWorkItemBuilder>();
            _workflowPathNavigator = Substitute.For<IWorkflowPathNavigator>();
            _dataStore = Substitute.For<IDataStore>();

            _stateMachine = new ForkStateMachine(_repository, _workItemBuilder, _workflowPathNavigator, _dataStore);
            _engine = Substitute.For<IEngine>();
        }

        [Fact]
        public void TransitToWaitingForChildren()
        {
            var workItem = new WorkItem
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.Created,
                Type = WorkflowType.Fork
            };

            _stateMachine.Transit(workItem, _engine);

            _repository.Received(1).DeleteChildren(workItem.Id);
            _repository.ReceivedWithAnyArgs(1).AddAll(null);

            Assert.Equal(WorkItemStatus.WaitingForChildren, workItem.Status);
            _engine.Received(1).Kick(workItem.Id);
        }

        [Fact]
        public void TransitToCompleted()
        {
            var workItem = new WorkItem
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.WaitingForChildren,
                Type = WorkflowType.Fork,
                ParentId = Helpers.Integer()
            };

            var parent = new WorkItem();
            _repository.GetParent(workItem).Returns(parent);

            _repository.CountInProgressChildren(workItem.Id).Returns(0);
            _workflowPathNavigator.Find(null).ReturnsForAnyArgs(Helpers.BuildFork(2));
            _repository.FindRunnableChildrenByOrder(workItem.Id, 2).ReturnsForAnyArgs(Enumerable.Empty<WorkItem>());
            var outputIds = new[] {1, 2};
            _repository.LoadChildOutputIds(workItem.Id).Returns(outputIds);
            var outputId = Helpers.Integer();
            _dataStore.AddReferences(workItem.JobId, outputIds, typeof (int)).Returns(outputId);

            _stateMachine.Transit(workItem, _engine);

            Assert.Equal(outputId, workItem.OutputId);
            Assert.Equal(WorkItemStatus.Completed, workItem.Status);

            _engine.Received(1).Kick(workItem.ParentId);
        }

        [Fact]
        public void KicksOffChildWorkItemsIfNumberOfInProgressIsLessThanMaxWorkers()
        {
            var workItem = new WorkItem
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.WaitingForChildren,
                Type = WorkflowType.Fork
            };

            var child = new WorkItem
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.Created,
                Type = WorkflowType.Activity
            };

            _repository.CountInProgressChildren(workItem.Id).Returns(0);
            _workflowPathNavigator.Find(null).ReturnsForAnyArgs(Helpers.BuildFork(2));
            _repository.FindRunnableChildrenByOrder(workItem.Id, 2).ReturnsForAnyArgs(new[] {child});

            _stateMachine.Transit(workItem, _engine);

            _engine.Received(1).Kick(child.Id);
        }

        [Fact]
        public void ShouldNotContinueIfHasAnyFailedChild()
        {
            var workItem = new WorkItem
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.WaitingForChildren,
                Type = WorkflowType.Fork
            };

            _repository.HasFailedChildren(Arg.Any<int>()).Returns(true);
            _stateMachine.Transit(workItem, _engine);

            _engine.DidNotReceiveWithAnyArgs().Kick(null);
        }
    }
}