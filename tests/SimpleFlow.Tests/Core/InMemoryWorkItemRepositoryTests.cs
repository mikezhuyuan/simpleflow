using System.Linq;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class InMemoryWorkItemRepositoryTests
    {
        readonly IWorkItemRepository _repository = new InMemoryWorkItemRepository();

        [Fact]
        public void TestLoadChildOutputIds()
        {
            var ids = Helpers.Integers(3).ToArray();

            _repository.Add(new WorkItem {ParentId = 1, Order = 1, OutputId = ids[0]});
            _repository.Add(new WorkItem {ParentId = 1, Order = 2, OutputId = ids[1]});
            _repository.Add(new WorkItem {ParentId = 1, Order = 3, OutputId = ids[2]});

            Assert.Equal(ids, _repository.LoadChildOutputIds(1));
        }

        [Fact]
        public void TestDeleteChildren()
        {
            var id = _repository.Add(new WorkItem {ParentId = 1, Order = 1});

            _repository.DeleteChildren(1);

            Assert.Null(_repository.Get(id));
        }

        [Fact]
        public void TestAddAll()
        {
            _repository.AddAll(new[] {new WorkItem {ParentId = 1, Order = 1}, new WorkItem {ParentId = 1, Order = 2}});

            Assert.NotNull(_repository.Get(1));
            Assert.NotNull(_repository.Get(2));
        }

        [Fact]
        public void TestGetChildByOrder()
        {
            _repository.Add(new WorkItem {ParentId = 1, Order = 1});
            var id = _repository.Add(new WorkItem {ParentId = 1, Order = 2});

            Assert.Equal(id, _repository.GetChildByOrder(1, 2).Id);
        }

        [Fact]
        public void TestGetLastChildByOrder()
        {
            _repository.Add(new WorkItem {ParentId = 1, Order = 1});
            var id = _repository.Add(new WorkItem {ParentId = 1, Order = 2});

            Assert.Equal(id, _repository.GetLastChildByOrder(1).Id);
        }

        [Fact]
        public void TestFindRunnableChildrenByOrder()
        {
            _repository.AddAll(new[]
            {
                new WorkItem {ParentId = 1, Order = 1, Status = WorkItemStatus.Running},
                new WorkItem {ParentId = 1, Order = 2, Status = WorkItemStatus.WaitingForChildren},
                new WorkItem {ParentId = 1, Order = 3, Status = WorkItemStatus.Created},
                new WorkItem {ParentId = 1, Order = 4, Status = WorkItemStatus.Created}
            });

            var items = _repository.FindRunnableChildrenByOrder(1, 1);

            Assert.Equal(3, items.Single().Order);

            items = _repository.FindRunnableChildrenByOrder(1, 2);

            Assert.Equal(2, items.Count);

            items = _repository.FindRunnableChildrenByOrder(1, 10);

            Assert.Equal(2, items.Count);
        }

        [Fact]
        public void TestCountInProgressChildren()
        {
            _repository.AddAll(new[]
            {
                new WorkItem {ParentId = 1, Order = 1, Status = WorkItemStatus.Running},
                new WorkItem {ParentId = 1, Order = 2, Status = WorkItemStatus.WaitingForChildren},
                new WorkItem {ParentId = 1, Order = 3, Status = WorkItemStatus.Pending},
                new WorkItem {ParentId = 1, Order = 4, Status = WorkItemStatus.Completed},
                new WorkItem {ParentId = 1, Order = 5, Status = WorkItemStatus.Created}
            });

            Assert.Equal(3, _repository.CountInProgressChildren(1));
        }
    }
}