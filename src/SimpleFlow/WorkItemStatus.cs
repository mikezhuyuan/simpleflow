namespace SimpleFlow.Core
{
    public enum WorkItemStatus
    {
        Created = 0,
        Pending = 1,
        Running = 2,
        WaitingForChildren = 3,
        Completed = 4,
        Failed = 5
    }

    static class WorkItemStatusExtensions
    {
        public static bool IsFinal(this WorkItemStatus s)
        {
            return s == WorkItemStatus.Failed || s == WorkItemStatus.Completed;
        }
    }
}