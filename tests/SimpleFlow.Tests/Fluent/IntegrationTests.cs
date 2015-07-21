using System;
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
            Func<string, string[]> split = _ => _.Split(',');
            Func<string, int> parse = int.Parse;

            var r = FluentFlow.Sequence()
                .Begin(split)
                .Then(FluentFlow.Fork()
                    .ForEach(FluentFlow.Sequence()
                        .Begin(FluentFlow.Parallel()
                            .Do(parse)
                            .Do(_ => _)
                            .Join())
                        .Then(_ => new User {Id = _.Item1, Name = _.Item2})
                        .End()))
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