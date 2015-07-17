using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Core;
using SimpleFlow.Fluent;
using Xunit;

namespace SimpleFlow.Tests.Fluent
{
    public class ActivityTests
    {
        [Fact]
        public void Test()
        {
            var a = FluentFlow.Activity<string, int>(s => int.Parse(s)).BuildBlock();

            Assert.Equal(typeof(string), a.InputTypes.Single());
            Assert.Equal(typeof(int), a.OutputType);

            Assert.Equal("Activity", a.ToString());
        }

        [Fact]
        public void TestAsync()
        {
            var a = FluentFlow.Activity<string, int>(s => Task.FromResult(int.Parse(s))).BuildBlock();

            Assert.Equal(typeof(string), a.InputTypes.Single());
            Assert.Equal(typeof(int), a.OutputType);

            Assert.Equal("Activity", a.ToString());
        }
    }
}