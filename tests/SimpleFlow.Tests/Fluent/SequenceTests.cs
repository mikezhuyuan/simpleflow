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
            var r = FluentFlow
                .Sequence<string>()
                .Then(int.Parse) //todo: first action should be Do?
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
            var r = FluentFlow
                .Sequence<string>()
                .Then(Task.FromResult)
                .End()
                .BuildBlock();

            Assert.Equal("Sequence(Activity)", r.ToString());
        }

        [Fact]
        public void TestNested()
        {
            var f1 = FluentFlow
                .Sequence<string>()
                .Then(int.Parse)
                .End();

            var f2 = FluentFlow
                .Sequence<string>()
                .Then(f1)
                .End();

            var r = f2.BuildBlock();

            Assert.Equal(typeof (string), r.InputTypes.Single());
            Assert.Equal(typeof (int), r.OutputType);

            Assert.Equal("Sequence(Sequence(Activity))", r.ToString());
        }
    }
}