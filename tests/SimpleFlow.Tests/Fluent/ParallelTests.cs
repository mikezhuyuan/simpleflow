using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Core;
using SimpleFlow.Fluent;
using Xunit;

namespace SimpleFlow.Tests.Fluent
{
    public class ParallelTests
    {
        [Fact]
        public void Test()
        {
            Func<int, string> toStr = _ => _.ToString();

            var f = FluentFlow.Parallel(2)
                .Do(toStr)
                .Do(_ => _.ToString())
                .Join();

            var r = f.BuildBlock();

            Assert.Equal(typeof (int), r.InputTypes.Single());
            Assert.Equal(typeof (IEnumerable<object>), r.OutputType);
            Assert.Equal(2, GetMaxWorkers(r));

            Assert.Equal("Parallel(Activity,Activity)", r.ToString());
        }

        [Fact]
        public void TestAsync()
        {
            Func<string, Task<int>> parseAsync = _ => Task.FromResult(int.Parse(_));

            var f = FluentFlow.Parallel(2)
                .Do(parseAsync)
                .Do(parseAsync)
                .Join();

            var r = f.BuildBlock();

            Assert.Equal("Parallel(Activity,Activity)", r.ToString());
        }

        [Fact]
        public void TestNested()
        {
            Func<string, Task<int>> parseAsync = _ => Task.FromResult(int.Parse(_));

            var f = FluentFlow.Parallel()
                .Do(
                    FluentFlow.Parallel()
                        .Do(parseAsync)
                        .Do(parseAsync)
                        .Join()
                )
                .Do(
                    FluentFlow.Parallel()
                        .Do(parseAsync)
                        .Do(parseAsync)
                        .Join()
                )
                .Join();

            var r = f.BuildBlock();

            Assert.Equal(typeof (string), r.InputTypes.Single());
            Assert.Equal(typeof (IEnumerable<object>), r.OutputType);

            Assert.Equal("Parallel(Parallel(Activity,Activity),Parallel(Activity,Activity))", r.ToString());
        }

        [Fact]
        public void TestVariousArguments()
        {
            Func<int, string> toStr = _ => _.ToString();

            var r = FluentFlow.Parallel(2)
                .Do(toStr)
                .Do(toStr)
                .Do(Task.FromResult)
                .Join().BuildBlock();

            Assert.Equal(2, GetMaxWorkers(r));
            Assert.Equal("Parallel(Activity,Activity,Activity)", r.ToString());

            r = FluentFlow.Parallel(2)
                .Do(toStr)
                .Do(toStr)
                .Do(toStr)
                .Do(Task.FromResult)
                .Join().BuildBlock();

            Assert.Equal(2, GetMaxWorkers(r));
            Assert.Equal("Parallel(Activity,Activity,Activity,Activity)", r.ToString());

            r = FluentFlow.Parallel(2)
                .Do(toStr)
                .Do(toStr)
                .Do(toStr)
                .Do(toStr)
                .Do(Task.FromResult)
                .Join().BuildBlock();

            Assert.Equal(2, GetMaxWorkers(r));
            Assert.Equal("Parallel(Activity,Activity,Activity,Activity,Activity)", r.ToString());
        }

        int GetMaxWorkers(WorkflowBlock workflowBlock)
        {
            return ((ParallelBlock) workflowBlock).MaxWorkers;
        }
    }
}