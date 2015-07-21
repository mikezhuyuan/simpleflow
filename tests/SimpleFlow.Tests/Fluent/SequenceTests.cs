using System;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Fluent;
using Xunit;

namespace SimpleFlow.Tests.Fluent
{
    public class SequenceTests
    {
        [Fact]
        public void Test()
        {
            Func<string, int> parse = int.Parse;
            var r = FluentFlow
                .Sequence()
                .Begin(parse)
                .Then(_ => _.ToString())
                .Then(int.Parse)
                .End()
                .BuildBlock();

            Assert.Equal(typeof (string), r.InputTypes.Single());
            Assert.Equal(typeof (int), r.OutputType);

            Assert.Equal("Sequence(Activity,Activity,Activity)", r.ToString());
        }

        [Fact]
        public void TestAsync()
        {
            Func<string, Task<int>> parseAsync = _ => Task.FromResult(int.Parse(_));
            var r = FluentFlow
                .Sequence()
                .Begin(parseAsync)
                .End()
                .BuildBlock();

            Assert.Equal("Sequence(Activity)", r.ToString());
        }

        [Fact]
        public void TestNested()
        {
            Func<string, int> parse = int.Parse;

            var f1 = FluentFlow
                .Sequence()
                .Begin(parse)
                .End();

            var f2 = FluentFlow
                .Sequence()
                .Begin(f1)
                .End();

            var r = f2.BuildBlock();

            Assert.Equal(typeof (string), r.InputTypes.Single());
            Assert.Equal(typeof (int), r.OutputType);

            Assert.Equal("Sequence(Sequence(Activity))", r.ToString());
        }
    }
}