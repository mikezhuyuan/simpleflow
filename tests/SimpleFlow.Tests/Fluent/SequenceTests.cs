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
            var r = FluentFlow.Input<string>()
                .Sequence()
                .Then(int.Parse) //todo: first action should be Do?
                .Then(_ => _.ToString())
                .Then(int.Parse)
                .End()
                .Build2();

            Assert.Equal(typeof (string), r.InputTypes.Single());
            Assert.Equal(typeof (int), r.OutputType);

            Assert.Equal("Sequence(Activity,Activity,Activity)", r.ToString());
        }

        [Fact]
        public void TestAsync()
        {
            var r = FluentFlow.Input<string>()
                .Sequence()
                .Then(Task.FromResult)
                .End()
                .Build2();

            Assert.Equal("Sequence(Activity)", r.ToString());
        }

        [Fact]
        public void TestNested()
        {
            var f1 = FluentFlow.Input<string>()
                .Sequence()
                .Then(int.Parse)
                .End();

            var f2 = FluentFlow.Input<string>()
                .Sequence()
                .Then(f1)
                .End();

            var r = f2.Build2();

            Assert.Equal(typeof (string), r.InputTypes.Single());
            Assert.Equal(typeof (int), r.OutputType);

            Assert.Equal("Sequence(Sequence(Activity))", r.ToString());
        }
    }
}