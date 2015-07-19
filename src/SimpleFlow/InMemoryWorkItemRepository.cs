using System.Collections.Generic;
using System.Linq;

namespace SimpleFlow.Core
{
    class InMemoryWorkItemRepository : IWorkItemRepository //todo: move to another project for data access
    {
        readonly IDictionary<int, WorkItem> _dataStore = new Dictionary<int, WorkItem>();

        public void Update(WorkItem workItem)
        {
            lock (_dataStore)
            {
                _dataStore[workItem.Id] = (WorkItem) workItem.Clone();
            }
        }

        public WorkItem Get(int id)
        {
            lock (_dataStore)
            {
                if (_dataStore.ContainsKey(id))
                    return (WorkItem) _dataStore[id].Clone();
            }

            return null;
        }

        public WorkItem GetParent(WorkItem workItem)
        {
            if (workItem.ParentId == null)
                return null;

            lock (_dataStore)
            {
                return _dataStore.Values.Single(_ => _.Id == workItem.ParentId);
            }
        }

        public IEnumerable<int> LoadChildOutputIds(int parentId)
        {
            lock (_dataStore)
            {
                return _dataStore.Values.Where(_ => _.ParentId == parentId).Select(_ => _.OutputId.Value).ToArray();
            }
        }

        public void DeleteChildren(int parentId)
        {
            lock (_dataStore)
            {
                var ids = _dataStore.Values.Where(_ => _.ParentId == parentId).Select(_ => _.Id).ToArray();

                foreach (var id in ids)
                {
                    _dataStore.Remove(id);
                }
            }
        }

        public int Add(WorkItem workItem)
        {
            lock (_dataStore)
            {
                var maxId = _dataStore.Keys.Any() ? _dataStore.Keys.Max() : 0;
                var id = maxId + 1;
                workItem.Id = id;
                _dataStore[id] = (WorkItem) workItem.Clone();

                return id;
            }
        }

        public void AddAll(IEnumerable<WorkItem> workItems)
        {
            lock (_dataStore)
            {
                var maxId = _dataStore.Keys.Any() ? _dataStore.Keys.Max() : 0;
                var id = maxId + 1;

                foreach (var workItem in workItems)
                {
                    workItem.Id = id++;
                    _dataStore[workItem.Id] = (WorkItem) workItem.Clone();
                }
            }
        }

        public WorkItem GetChildByOrder(int parentId, int order)
        {
            WorkItem item;
            lock (_dataStore)
            {
                item = _dataStore.Values.SingleOrDefault(_ => _.ParentId == parentId && _.Order == order);
            }

            if (item == null)
                return null;

            return (WorkItem) item.Clone();
        }

        public WorkItem GetLastChildByOrder(int parentId)
        {
            WorkItem item;
            lock (_dataStore)
            {
                item = _dataStore.Values.Where(_ => _.ParentId == parentId).OrderBy(_ => _.Order).LastOrDefault();
            }

            if (item == null)
                return null;

            return (WorkItem) item.Clone();
        }

        public ICollection<WorkItem> FindRunnableChildrenByOrder(int parentId, int count)
        {
            lock (_dataStore)
            {
                return
                    _dataStore.Values.Where(_ => _.ParentId == parentId && _.Status == WorkItemStatus.Created)
                        .OrderBy(_ => _.Order)
                        .Take(count)
                        .Select(_ => (WorkItem) _.Clone())
                        .ToArray();
            }
        }

        public int CountInProgressChildren(int parentId)
        {
            lock (_dataStore)
            {
                return
                    _dataStore.Values
                        .Count(
                            _ =>
                                _.ParentId == parentId && _.Status != WorkItemStatus.Created &&
                                _.Status != WorkItemStatus.Completed);
            }
        }

        public bool HasFailedChildren(int parentId)
        {
            lock (_dataStore)
            {
                return _dataStore.Values.Any(_ => _.ParentId == parentId && _.Status == WorkItemStatus.Failed);
            }
        }
    }
}