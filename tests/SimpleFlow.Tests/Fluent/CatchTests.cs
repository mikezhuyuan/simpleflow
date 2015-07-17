using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleFlow.Fluent;
using Xunit;

namespace SimpleFlow.Tests.Fluent
{
    public class CatchTests
    {
        [Fact]
        public void TestActivity()
        {
            var handler = Helpers.ExceptionHandler<int>();
            var r = FluentFlow.Activity<string, int>(s => Task.FromResult(int.Parse(s))).Catch(handler).BuildBlock();

            Assert.Equal(handler, r.ExceptionHandler);
        }

        [Fact]
        public void TestSequence()
        {
            var handler = Helpers.ExceptionHandler<int>();
            var r = FluentFlow
               .Sequence<string>()
               .Then(int.Parse)
               .End()
               .Catch(handler)
               .BuildBlock();

            Assert.Equal(handler, r.ExceptionHandler);
        }

        [Fact]
        public void TestFork()
        {
            var handler = Helpers.ExceptionHandler<IEnumerable<int>>();
            var r = FluentFlow
                .Fork<IEnumerable<string>>(2)
                .ForEach(int.Parse)
                .Join()
                .Catch(handler)
                .BuildBlock();

            Assert.Equal(handler, r.ExceptionHandler);
        }

        [Fact]
        public void TestParallel()
        {
            var handler = Helpers.ExceptionHandler<Tuple<string, string>>();
            var r = FluentFlow.Parallel<int>(2)
                .Do(_ => _.ToString())
                .Do(_ => _.ToString())
                .Join()
                .Catch(handler)
                .BuildBlock();

            Assert.Equal(handler, r.ExceptionHandler);
        }
    }
}