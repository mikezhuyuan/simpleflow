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
            var r = Flow.Activity<string, int>(s => Task.FromResult(int.Parse(s))).Catch(handler).BuildBlock();

            Assert.Equal(handler, r.ExceptionHandler);
        }

        [Fact]
        public void TestSequence()
        {
            Func<string, int> parse = int.Parse;

            var handler = Helpers.ExceptionHandler<int>();
            var r = Flow
                .Sequence()
                .Begin(parse)
                .End()
                .Catch(handler)
                .BuildBlock();

            Assert.Equal(handler, r.ExceptionHandler);
        }

        [Fact]
        public void TestFork()
        {
            Func<string, int> parse = int.Parse;

            var handler = Helpers.ExceptionHandler<IEnumerable<int>>();
            var r = Flow
                .Fork(2)
                .ForEach(parse)
                .Catch(handler)
                .BuildBlock();

            Assert.Equal(handler, r.ExceptionHandler);
        }

        [Fact]
        public void TestParallel()
        {
            Func<int, string> toStr = _ => _.ToString();

            var handler = Helpers.ExceptionHandler<Tuple<string, string>>();
            var r = Flow.Parallel(2)
                .Do(toStr)
                .Do(toStr)
                .Join()
                .Catch(handler)
                .BuildBlock();

            Assert.Equal(handler, r.ExceptionHandler);
        }
    }
}