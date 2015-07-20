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
        public async Task KickGroupItem()
        {
            var sequence = new WorkItem(1, null, 0, WorkflowType.Sequence, "root")
            {
                Id = Helpers.Integer(),
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

            _engine.Kick(sequence.Id);

            await _engine.Completion;

            Assert.Equal(WorkItemStatus.Completed, sequence.Status);
        }
    }
}