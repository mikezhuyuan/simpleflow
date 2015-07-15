using System;

namespace SimpleFlow.Core
{
    public class Workflow<TInput, TOutput>
    {
        public Workflow(string name, WorkflowBlock root)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (root == null) throw new ArgumentNullException("root");

            Name = name;
            Root = root;
        }

        public string Name { get; private set; }
        public WorkflowBlock Root { get; private set; }
    }
}