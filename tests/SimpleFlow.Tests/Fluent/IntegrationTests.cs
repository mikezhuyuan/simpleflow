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
            var r = FluentFlow.Sequence<string>()
                .Then(_ => _.Split(','))
                .Then(FluentFlow.Fork<IEnumerable<string>>()
                    .ForEach(FluentFlow.Sequence<string>()
                        .Then(FluentFlow.Parallel<string>()
                            .Do(int.Parse)
                            .Do(_ => _)
                            .Join())
                        .Then(_ => new User {Id = _.Item1, Name = _.Item2})
                        .End())
                    .Join())
                .End()
                .BuildBlock();

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