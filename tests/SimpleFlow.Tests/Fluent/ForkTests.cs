using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Core;
using SimpleFlow.Fluent;
using Xunit;

namespace SimpleFlow.Tests.Fluent
{
    public class ForkTests
    {
        [Fact]
        public void Test()
        {
            var f = FluentFlow.Input<IEnumerable<string>>()
                .Fork(2)
                .ForEach(int.Parse)
                .Join();

            var r = f.BuildBlock();

            Assert.Equal(typeof (string), r.InputTypes.Single());
            Assert.Equal(typeof (IEnumerable<int>), r.OutputType);
            Assert.Equal(2, ((ForkBlock) r).MaxWorkers);

            Assert.Equal("Fork(Activity)", r.ToString());
        }

        [Fact]
        public void TestAsync()
        {
            var f = FluentFlow.Input<IEnumerable<string>>()
               .Fork(2)
               .ForEach(Task.FromResult)
               .Join();

            var r = f.BuildBlock();

            Assert.Equal("Fork(Activity)", r.ToString());
        }

        [Fact]
        public void TestNested()
        {
            var f = FluentFlow.Input<IEnumerable<IEnumerable<string>>>()
                .Fork()
                .ForEach(FluentFlow.Input<IEnumerable<string>>()
                    .Fork()
                    .ForEach(int.Parse)
                    .Join())
                .Join();

            var r = f.BuildBlock();

            Assert.Equal("Fork(Fork(Activity))", r.ToString());
        }
    }
}