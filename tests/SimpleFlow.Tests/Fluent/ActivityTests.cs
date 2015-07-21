using System;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Fluent;
using Xunit;

namespace SimpleFlow.Tests.Fluent
{
    public class ActivityTests
    {
        [Fact]
        public void Test()
        {
            Func<string, int> func = int.Parse;

            var a = FluentFlow.Activity(func).BuildBlock();

            Assert.Equal(typeof (string), a.InputTypes.Single());
            Assert.Equal(typeof (int), a.OutputType);

            Assert.Equal("Activity", a.ToString());
        }

        [Fact]
        public void TestAsync()
        {
            Func<string, Task<int>> parseAsync = _ => Task.FromResult(int.Parse(_));

            var a = FluentFlow.Activity(parseAsync).BuildBlock();

            Assert.Equal(typeof (string), a.InputTypes.Single());
            Assert.Equal(typeof (int), a.OutputType);

            Assert.Equal("Activity", a.ToString());
        }
    }
}