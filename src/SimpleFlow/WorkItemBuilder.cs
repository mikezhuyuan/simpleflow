using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFlow.Core
{
    interface IWorkItemBuilder
    {
        IEnumerable<WorkItem> BuildChildren(WorkItem workItem);
    }

    class WorkItemBuilder : IWorkItemBuilder
    {
        readonly IDataStore _dataStore;
        readonly IWorkflowPathNavigator _pathNavigator;

        public WorkItemBuilder(IWorkflowPathNavigator pathNavigator, IDataStore dataStore)
        {
            if (pathNavigator == null) throw new ArgumentNullException("pathNavigator");
            if (dataStore == null) throw new ArgumentNullException("dataStore");

            _pathNavigator = pathNavigator;
            _dataStore = dataStore;
        }

        public IEnumerable<WorkItem> BuildChildren(WorkItem workItem)
        {
            if (workItem.Type == WorkflowType.Fork)
                return BuildForFork(workItem);

            if (workItem.Type == WorkflowType.Sequence)
                return BuildForSequence(workItem);

            if (workItem.Type == WorkflowType.Parallel)
                return BuildForPallaral(workItem);

            throw new ArgumentException("workItem.Type not supported");
        }

        IEnumerable<WorkItem> BuildForSequence(WorkItem workItem)
        {
            var definition = (SequenceBlock) _pathNavigator.Find(workItem.WorkflowPath);

            var children = definition.Children
                .Select(
                    (item, index) =>
                        new WorkItem(workItem.JobId, workItem.Id, index, item.Type, _pathNavigator.Path(item)))
                .ToList();

            var first = children.First();

            first.InputId = workItem.InputId;
            return children;
        }

        IEnumerable<WorkItem> BuildForFork(WorkItem workItem)
        {
            var definition = (ForkBlock) _pathNavigator.Find(workItem.WorkflowPath);
            var inputIds = _dataStore.SplitAndGetIds(workItem.InputId.Value);

            var children =
                inputIds.Select(
                    (id, index) =>
                        new WorkItem(workItem.JobId, workItem.Id, index, definition.Child.Type,
                            _pathNavigator.Path(definition.Child)) {InputId = id})
                    .ToArray();

            return children;
        }

        IEnumerable<WorkItem> BuildForPallaral(WorkItem workItem)
        {
            var definition = (ParallelBlock) _pathNavigator.Find(workItem.WorkflowPath);

            var children = definition.Children.Select(
                (child, index) =>
                    new WorkItem(workItem.JobId, workItem.Id, index, child.Type, _pathNavigator.Path(child))
                    {
                        InputId = workItem.InputId
                    }).ToArray();

            return children;
        }
    }
}