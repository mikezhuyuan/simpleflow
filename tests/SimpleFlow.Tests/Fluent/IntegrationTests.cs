using System.Collections.Generic;
using System.Linq;
using SimpleFlow.Fluent;
using Xunit;

namespace SimpleFlow.Tests.Fluent
{
    public class IntegrationTests
    {
        [Fact]
        public void Test()
        {
            var r = FluentFlow.Input<string>()
                .Sequence()
                .Then(_ => _.Split(',').AsEnumerable())
                .Then(FluentFlow.Input<IEnumerable<string>>()
                    .Fork()
                    .ForEach(FluentFlow.Input<string>()
                        .Parallel()
                        .Do(int.Parse)
                        .Do(_ => _)
                        .Join((id, name) => new User()))
                    .Join())
                .End()
                .Build2();

            Assert.Equal(typeof (string), r.InputTypes.Single());
            Assert.Equal(typeof (IEnumerable<User>), r.OutputType);

            Assert.Equal("Sequence(Activity,Fork(Sequence(Parallel(Activity,Activity),Activity)))", r.ToString());
        }

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}