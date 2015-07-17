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
            var f = FluentFlow.Parallel<int>(2)
                .Do(_ => _.ToString())
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
            var f = FluentFlow.Parallel<int>(2)
                .Do(Task.FromResult)
                .Do(Task.FromResult)
                .Join();

            var r = f.BuildBlock();

            Assert.Equal("Parallel(Activity,Activity)", r.ToString());
        }

        [Fact]
        public void TestNested()
        {
            var f = FluentFlow.Parallel<int>()
                .Do(
                    FluentFlow.Parallel<int>()
                        .Do(_ => _.ToString())
                        .Do(_ => _.ToString())
                        .Join()
                )
                .Do(
                    FluentFlow.Parallel<int>()
                        .Do(_ => _.ToString())
                        .Do(_ => _.ToString())
                        .Join()
                )
                .Join();

            var r = f.BuildBlock();

            Assert.Equal(typeof (int), r.InputTypes.Single());
            Assert.Equal(typeof (IEnumerable<object>), r.OutputType);

            Assert.Equal("Parallel(Parallel(Activity,Activity),Parallel(Activity,Activity))", r.ToString());
        }

        [Fact]
        public void TestVariousArguments()
        {
            var r = FluentFlow.Parallel<int>(2)
                .Do(_ => _)
                .Do(_ => _)
                .Do(Task.FromResult)
                .Join().BuildBlock();

            Assert.Equal(2, GetMaxWorkers(r));
            Assert.Equal("Parallel(Activity,Activity,Activity)", r.ToString());

            r = FluentFlow.Parallel<int>(2)
                .Do(_ => _)
                .Do(_ => _)
                .Do(_ => _)
                .Do(Task.FromResult)
                .Join().BuildBlock();

            Assert.Equal(2, GetMaxWorkers(r));
            Assert.Equal("Parallel(Activity,Activity,Activity,Activity)", r.ToString());

            r = FluentFlow.Parallel<int>(2)
                .Do(_ => _)
                .Do(_ => _)
                .Do(_ => _)
                .Do(_ => _)
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