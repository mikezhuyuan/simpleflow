using System;

namespace SimpleFlow.Core
{
    public class WorkItem : ICloneable
    {
        internal WorkItem()
        {
        }

        public WorkItem(int jobId, int? parentId, int order, WorkflowType type, string path)
        {
            JobId = jobId;
            ParentId = parentId;
            Order = order;
            Type = type;
            WorkflowPath = path;
            Status = WorkItemStatus.Created;
        }

        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int JobId { get; set; }
        public int Order { get; set; }
        public WorkItemStatus Status { get; set; }
        public WorkflowType Type { get; set; }
        public int? InputId { get; set; }
        public int? OutputId { get; set; }
        public string WorkflowPath { get; set; }
        public int? ExceptionId { get; set; }

        public object Clone()
        {
            return (WorkItem) MemberwiseClone();
        }

        public WorkItem Retry()
        {
            var newItem = (WorkItem) Clone();
            newItem.Id = 0;
            newItem.Status = WorkItemStatus.Created;
            newItem.ExceptionId = null;
            newItem.OutputId = null;
            newItem.Order++;

            return newItem;
        }
    }
}