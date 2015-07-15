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
            var f = FluentFlow.Input<int>().Parallel(2)
                .Do(_ => _.ToString())
                .Join(_ => _);

            var r = f.BuildWorkflow();

            Assert.Equal(typeof (int), r.InputTypes.Single());
            Assert.Equal(typeof (string), r.OutputType);
            Assert.Equal(2, GetMaxWorkers(r));

            Assert.Equal("Sequence(Parallel(Activity),Activity)", r.ToString());
        }

        [Fact]
        public void TestAsync()
        {
            var f = FluentFlow.Input<int>().Parallel(2)
               .Do(Task.FromResult)
               .Join(_ => _);

            var r = f.BuildWorkflow();

            Assert.Equal("Sequence(Parallel(Activity),Activity)", r.ToString());
        }

        [Fact]
        public void TestNested()
        {
            var f = FluentFlow.Input<int>().Parallel()
                .Do(
                    FluentFlow.Input<int>().Parallel()
                        .Do(_ => _.ToString())
                        .Join(_ => _)
                )
                .Join(_ => _);

            var r = f.BuildWorkflow();

            Assert.Equal(typeof (int), r.InputTypes.Single());
            Assert.Equal(typeof (string), r.OutputType);

            Assert.Equal("Sequence(Parallel(Sequence(Parallel(Activity),Activity)),Activity)", r.ToString());
        }

        [Fact]
        public void TestVariousArguments()
        {
            var r = (FluentFlow.Input<int>().Parallel(2)
                .Do(_ => _)
                .Do(Task.FromResult)
                .Join((_1, _2) => 1)).BuildWorkflow();

            Assert.Equal(2, GetMaxWorkers(r));
            Assert.Equal("Sequence(Parallel(Activity,Activity),Activity)", r.ToString());

            r = (FluentFlow.Input<int>().Parallel(2)
                .Do(_ => _)
                .Do(_ => _)
                .Do(Task.FromResult)
                .Join((_1, _2, _3) => 1)).BuildWorkflow();

            Assert.Equal(2, GetMaxWorkers(r));
            Assert.Equal("Sequence(Parallel(Activity,Activity,Activity),Activity)", r.ToString());

            r = (FluentFlow.Input<int>().Parallel(2)
                .Do(_ => _)
                .Do(_ => _)
                .Do(_ => _)
                .Do(Task.FromResult)
                .Join((_1, _2, _3, _4) => 1)).BuildWorkflow();

            Assert.Equal(2, GetMaxWorkers(r));
            Assert.Equal("Sequence(Parallel(Activity,Activity,Activity,Activity),Activity)", r.ToString());

            r = (FluentFlow.Input<int>().Parallel(2)
                .Do(_ => _)
                .Do(_ => _)
                .Do(_ => _)
                .Do(_ => _)
                .Do(Task.FromResult)
                .Join((_1, _2, _3, _4, _5) => 1)).BuildWorkflow();

            Assert.Equal(2, GetMaxWorkers(r));
            Assert.Equal("Sequence(Parallel(Activity,Activity,Activity,Activity,Activity),Activity)", r.ToString());
        }

        int GetMaxWorkers(WorkflowBlock workflowBlock)
        {
            return ((ParallelBlock) ((SequenceBlock) workflowBlock).Children.First()).MaxWorkers;
        }
    }
}