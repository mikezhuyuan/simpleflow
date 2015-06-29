using System.Linq;
using NSubstitute;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class WorkflowItemBuilderTests
    {
        readonly IDataStore _dataStore;
        readonly IWorkflowPathNavigator _navigator;
        readonly IWorkItemBuilder _workItemBuilder;

        public WorkflowItemBuilderTests()
        {
            _navigator = Substitute.For<IWorkflowPathNavigator>();
            _dataStore = Substitute.For<IDataStore>();
            _workItemBuilder = new WorkItemBuilder(_navigator, _dataStore);
        }

        [Fact]
        public void SequenceChildren()
        {
            var a1 = Helpers.BuildActivity();
            var a2 = Helpers.BuildActivity();
            var c = new SequenceBlock(new[] {a1, a2});


            var wi = new WorkItem(Helpers.Integer(), Helpers.Integer(), 0, WorkflowType.Sequence, Helpers.String());
            wi.Id = Helpers.Integer();
            wi.InputId = Helpers.Integer();

            _navigator.Find(wi.WorkflowPath).Returns(c);

            var items = _workItemBuilder.BuildChildren(wi).ToArray();
            var item1 = items.First();
            var item2 = items.Last();

            Assert.Equal(2, items.Count());

            Assert.Equal(wi.JobId, item1.JobId);
            Assert.Equal(wi.Id, item1.ParentId);
            Assert.Equal(0, item1.Order);
            Assert.Equal(wi.InputId, item1.InputId);
            Assert.Equal(WorkflowType.Activity, item1.Type);

            Assert.Equal(wi.JobId, item2.JobId);
            Assert.Equal(wi.Id, item2.ParentId);
            Assert.Equal(1, item2.Order);
            Assert.Null(item2.InputId);
            Assert.Equal(WorkflowType.Activity, item2.Type);
        }

        [Fact]
        public void ForkChildren()
        {
            var a = Helpers.BuildActivity();
            var f = new ForkBlock(a, 1);

            var wi = new WorkItem(Helpers.Integer(), Helpers.Integer(), 0, WorkflowType.Fork, Helpers.String())
            {
                InputId = Helpers.Integer()
            };
            _navigator.Find(wi.WorkflowPath).Returns(f);
            _dataStore.SplitAndGetIds(wi.InputId.Value).Returns(new[] {1, 2});

            wi.Id = Helpers.Integer();

            var items = _workItemBuilder.BuildChildren(wi).ToArray();
            var item1 = items[0];
            var item2 = items[1];

            Assert.Equal(2, items.Count());

            Assert.Equal(wi.JobId, item1.JobId);
            Assert.Equal(wi.Id, item1.ParentId);
            Assert.Equal(0, item1.Order);
            Assert.Equal(1, item1.InputId);
            Assert.Equal(WorkflowType.Activity, item1.Type);

            Assert.Equal(wi.JobId, item2.JobId);
            Assert.Equal(wi.Id, item2.ParentId);
            Assert.Equal(1, item2.Order);
            Assert.Equal(2, item2.InputId);
            Assert.Equal(WorkflowType.Activity, item2.Type);

            _navigator.Received(2).Path(a);
        }

        [Fact]
        public void ParallelChildren()
        {
            var a = Helpers.BuildActivity();
            var b = Helpers.BuildActivity();
            var p = new ParallelBlock(new[] {a, b}, 1);

            var wi = new WorkItem(Helpers.Integer(), Helpers.Integer(), 0, WorkflowType.Parallel, Helpers.String())
            {
                InputId = Helpers.Integer()
            };

            _navigator.Find(wi.WorkflowPath).Returns(p);

            var items = _workItemBuilder.BuildChildren(wi).ToArray();
            var item1 = items[0];
            var item2 = items[1];

            Assert.Equal(2, items.Count());

            Assert.Equal(wi.JobId, item1.JobId);
            Assert.Equal(wi.Id, item1.ParentId);
            Assert.Equal(0, item1.Order);
            Assert.Equal(wi.InputId, item1.InputId);
            Assert.Equal(WorkflowType.Activity, item1.Type);

            Assert.Equal(wi.JobId, item2.JobId);
            Assert.Equal(wi.Id, item2.ParentId);
            Assert.Equal(1, item2.Order);
            Assert.Equal(wi.InputId, item2.InputId);
            Assert.Equal(WorkflowType.Activity, item2.Type);

            _navigator.Received(1).Path(p.Children.First());
            _navigator.Received(1).Path(p.Children.Last());
        }
    }
}