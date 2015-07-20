using NSubstitute;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class ActivityStateMachineTests
    {
        [Fact]
        public void Test()
        {
            var repository = Substitute.For<IWorkItemRepository>();
            var engine = Substitute.For<IEngine>();

            var machine = new ActivityStateMachine(repository);

            var workItem = new WorkItem
            {
                Id = Helpers.Integer(),
                Status = WorkItemStatus.Created
            };

            machine.Transit(workItem, engine);

            repository.Received(1).Update(Arg.Is<WorkItem>(_ => _.Status == WorkItemStatus.Pending));
            engine.Received(1).PostActivity(workItem.Id);
        }
    }
}