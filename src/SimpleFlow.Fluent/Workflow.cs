using System;
using SimpleFlow.Core;

namespace SimpleFlow.Fluent
{
    public class Workflow<TInput, TOutput>
    {
        internal Func<WorkflowBlock> BuildBlock { get; set; }

        public Core.Workflow<TInput, TOutput> Build(string name)
        {
            var root = BuildBlock();

            return new Core.Workflow<TInput, TOutput>(name, root);
        }

        public Workflow<TInput, TOutput> Catch(Func<Exception, TOutput> handler)
        {
            if (handler == null)
                handler = ex => default(TOutput);

            return new Workflow<TInput, TOutput>
            {
                BuildBlock = () =>
                {
                    var r = BuildBlock();

                    r.ExceptionHandler = handler;

                    return r;
                }
            };
        }

        public Workflow<TInput, TOutput> Retry(int count = 1)
        {
            return new Workflow<TInput, TOutput>
            {
                BuildBlock = () => new RetryBlock(BuildBlock(), count)
            };
        }
    }
}