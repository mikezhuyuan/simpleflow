using NSubstitute;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class RetryStateMachineTests //todo: add more unit tests to cover common paths
    {
        readonly IEngine _engine;
        readonly IWorkItemRepository _repository;
        readonly RetryStateMachine _stateMachine;
        readonly IWorkflowPathNavigator _workflowPathNavigator;
        readonly IWorkItemBuilder _workItemBuilder;

        public RetryStateMachineTests()
        {
            _repository = Substitute.For<IWorkItemRepository>();
            _workItemBuilder = Substitute.For<IWorkItemBuilder>();
            _workflowPathNavigator = Substitute.For<IWorkflowPathNavigator>();
            _stateMachine = new RetryStateMachine(_repository, _workItemBuilder, _workflowPathNavigator);
            _engine = Substitute.For<IEngine>();
        }

        [Fact]
        public void ShouldRetryIfChildFailed()
        {
            var parent = new WorkItem(1, Helpers.Integer(), 0, WorkflowType.Activity, Helpers.String())
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.WaitingForChildren,
                Type = WorkflowType.Retry
            };
            var child = new WorkItem(1, parent.Id, 0, WorkflowType.Activity, "child")
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.Failed
            };

            _repository.GetLastChildByOrder(parent.Id).Returns(child);

            _workflowPathNavigator.Find(parent.WorkflowPath).Returns(Helpers.BuildRetry(2));

            _stateMachine.Transit(parent, _engine);

            _repository.Received(1).Add(Arg.Is<WorkItem>(_ => _.Status == WorkItemStatus.Created));
            _engine.ReceivedWithAnyArgs(1).Kick(null);
        }

        [Fact]
        public void ShouldThrowExceptionIfExceedsMaxRetryCount()
        {
            var parent = new WorkItem(1, Helpers.Integer(), 0, WorkflowType.Activity, Helpers.String())
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.WaitingForChildren,
                Type = WorkflowType.Retry
            };

            var child = new WorkItem(1, parent.Id, 2, WorkflowType.Activity, Helpers.String())
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.Failed,
                ExceptionId = Helpers.Integer()
            };

            _repository.GetLastChildByOrder(parent.Id).Returns(child);

            _workflowPathNavigator.Find(parent.WorkflowPath).Returns(Helpers.BuildRetry());

            _stateMachine.Transit(parent, _engine);

            _repository.Received(1)
                .Update(Arg.Is<WorkItem>(_ => _.ExceptionId == child.ExceptionId && _.Status == WorkItemStatus.Failed));
            _engine.Received(1).Rescure(parent.Id);
        }
    }
}