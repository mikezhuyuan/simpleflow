using System;
using System.Collections.Generic;

namespace SimpleFlow.Core
{
    interface IWorkflowPathNavigator
    {
        WorkflowBlock Find(string path);
        string Path(WorkflowBlock item);
    }

    class WorkflowPathNavigator : IWorkflowPathNavigator
    {
        readonly IDictionary<WorkflowBlock, string> _blockToPathMap;
        readonly IDictionary<string, WorkflowBlock> _pathToBlockMap;

        public WorkflowPathNavigator(WorkflowBlock workflowBlock)
        {
            if (workflowBlock == null) throw new ArgumentNullException("workflowBlock");

            _pathToBlockMap = new Dictionary<string, WorkflowBlock>();
            _blockToPathMap = new Dictionary<WorkflowBlock, string>();

            Visit(workflowBlock, "", 0);
        }

        public WorkflowBlock Find(string path)
        {
            if (_pathToBlockMap.ContainsKey(path))
                return _pathToBlockMap[path];

            return null;
        }

        public string Path(WorkflowBlock item)
        {
            if (_blockToPathMap.ContainsKey(item))
                return _blockToPathMap[item];

            return null;
        }

        void Visit(WorkflowBlock item, string parentPath, int index)
        {
            var path = string.Format("{0}{1}[{2}]", parentPath == "" ? "" : parentPath + ".", item.Type, index);

            _pathToBlockMap[path] = item;
            _blockToPathMap[item] = path;

            if (item is GroupBlock)
            {
                var children = ((GroupBlock) item).Children;
                var i = 0;
                foreach (var child in children)
                {
                    Visit(child, path, i++);
                }
            }
        }
    }
}