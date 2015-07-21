using System;
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
            Func<string, int> func = int.Parse;

            var f = FluentFlow
                .Fork(2)
                .ForEach(func);

            var r = f.BuildBlock();

            Assert.Equal(typeof (string), r.InputTypes.Single());
            Assert.Equal(typeof (IEnumerable<int>), r.OutputType);
            Assert.Equal(2, ((ForkBlock) r).MaxWorkers);

            Assert.Equal("Fork(Activity)", r.ToString());
        }

        [Fact]
        public void TestAsync()
        {
            Func<string, Task<int>> parseAsync = _ => Task.FromResult(int.Parse(_));
            
            var f = FluentFlow
                .Fork(2)
                .ForEach(parseAsync);

            var r = f.BuildBlock();

            Assert.Equal("Fork(Activity)", r.ToString());
        }

        [Fact]
        public void TestNested()
        {
            Func<string, int> parse = int.Parse;

            var f = FluentFlow
                .Fork()
                .ForEach(FluentFlow
                    .Fork()
                    .ForEach(parse));

            var r = f.BuildBlock();

            Assert.Equal("Fork(Fork(Activity))", r.ToString());
        }
    }
}