using System.Threading.Tasks;
using NSubstitute;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class EngineTests
    {
        readonly IActivityRunner _activityRunner;
        readonly IEngine _engine;
        readonly IWorkItemRepository _repository = new InMemoryWorkItemRepository();
        readonly IStateMachineProvider _stateMachineProvider;

        public EngineTests()
        {
            _activityRunner = Substitute.For<IActivityRunner>();
            _stateMachineProvider = Substitute.For<IStateMachineProvider>();
            _engine = new Engine(_repository, _activityRunner, _stateMachineProvider);
        }

        [Fact]
        public async Task KickActivity()
        {
            var activity = new WorkItem(1, null, 0, WorkflowType.Activity, "root")
            {
                Status = WorkItemStatus.Created
            };

            _engine.Kick(activity);

            await _engine.Completion;

            _activityRunner.Received(1).Run(Arg.Is<WorkItem>(wi => wi.Id == activity.Id));
        }

        [Fact]
        public async Task KickGroupItem()
        {
            var sequence = new WorkItem(1, null, 0, WorkflowType.Sequence, "root")
            {
                Status = WorkItemStatus.WaitingForChildren
            };

            _repository.Add(sequence);

            var machine = Substitute.For<IStateMachine>();

            _stateMachineProvider.Resolve(null).ReturnsForAnyArgs(machine);

            machine.Transit(Arg.Any<WorkItem>(), Arg.Do<IEngine>(engine =>
            {
                sequence.Status = WorkItemStatus.Completed;
                _repository.Update(sequence);
                engine.Kick(null);
            }));

            _engine.Kick(sequence);

            await _engine.Completion;

            Assert.Equal(WorkItemStatus.Completed, sequence.Status);
        }
    }
}