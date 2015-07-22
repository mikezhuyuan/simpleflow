using System;
using System.Linq;
using SimpleFlow.Fluent;
using Xunit;

namespace SimpleFlow.Tests.Fluent
{
    public class RetryTests
    {
        [Fact]
        public void Test()
        {
            Func<string, int> func = int.Parse;

            var a = Flow.Activity(func).Retry().BuildBlock();

            Assert.Equal(typeof(string), a.InputTypes.Single());
            Assert.Equal(typeof(int), a.OutputType);

            Assert.Equal("Retry(Activity)", a.ToString());
        }
    }
}