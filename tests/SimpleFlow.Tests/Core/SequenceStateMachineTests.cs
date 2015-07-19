using System.Linq;
using NSubstitute;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class SequenceStateMachineTests
    {
        readonly IEngine _engine;
        readonly IWorkItemRepository _repository;
        readonly IStateMachine _stateMachine;
        readonly IWorkItemBuilder _workItemBuilder;

        public SequenceStateMachineTests()
        {
            _repository = Substitute.For<IWorkItemRepository>();
            _workItemBuilder = Substitute.For<IWorkItemBuilder>();
            _engine = Substitute.For<IEngine>();
            _stateMachine = new SequenceStateMachine(_repository, _workItemBuilder);
        }

        [Fact]
        public void TransitToWaitingForChildren()
        {
            var workItem = new WorkItem
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.Created,
                Type = WorkflowType.Sequence
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
                Type = WorkflowType.Sequence,
                ParentId = Helpers.Integer()
            };

            var last = new WorkItem {OutputId = Helpers.Integer()};
            _repository.CountInProgressChildren(workItem.Id).Returns(0);

            _repository.FindRunnableChildrenByOrder(workItem.Id, 1).Returns(Enumerable.Empty<WorkItem>());

            _repository.GetLastChildByOrder(workItem.Id).Returns(last);

            _stateMachine.Transit(workItem, _engine);

            _repository.Received(1).Update(workItem);

            Assert.Equal(WorkItemStatus.Completed, workItem.Status);

            _engine.Received(1).Kick(workItem.ParentId);
        }

        [Fact]
        public void KickOffNext()
        {
            var workItem = new WorkItem
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.WaitingForChildren,
                Type = WorkflowType.Sequence
            };
            var next = new WorkItem {Id = Helpers.Integer(), OutputId = Helpers.Integer(), Order = 1};
            var previous = new WorkItem {OutputId = Helpers.Integer()};

            _repository.CountInProgressChildren(workItem.Id).Returns(0);
            _repository.FindRunnableChildrenByOrder(workItem.Id, 1).Returns(new[] {next});
            _repository.GetChildByOrder(workItem.Id, next.Order - 1).Returns(previous);

            _stateMachine.Transit(workItem, _engine);

            Assert.Equal(previous.OutputId, next.InputId);
            _engine.Received(1).Kick(next.Id);
        }

        [Fact]
        public void ShouldNotContinueIfHasAnyFailedChild()
        {
            var workItem = new WorkItem
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.WaitingForChildren,
                Type = WorkflowType.Sequence
            };

            _repository.HasFailedChildren(Arg.Any<int>()).Returns(true);
            _stateMachine.Transit(workItem, _engine);

            _engine.DidNotReceiveWithAnyArgs().Kick(null);
        }
    }
}