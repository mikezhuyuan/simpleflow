using System.Collections.Generic;

namespace SimpleFlow.Core
{
    interface IWorkItemRepository
    {
        void Update(WorkItem workItem);
        WorkItem Get(int id);
        WorkItem GetParent(WorkItem workItem);
        IEnumerable<int> LoadChildOutputIds(int parentId);
        void DeleteChildren(int parentId);
        void AddAll(IEnumerable<WorkItem> workItem);
        int Add(WorkItem workItem);
        WorkItem GetChildByOrder(int parentId, int order);
        WorkItem GetLastChildByOrder(int parentId);
        ICollection<WorkItem> FindRunnableChildrenByOrder(int parentId, int count);
        int CountInProgressChildren(int parentId);
        bool HasFailedChildren(int parentId);
    }
}