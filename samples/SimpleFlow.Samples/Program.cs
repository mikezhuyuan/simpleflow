using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Fluent;

namespace SimpleFlow.Samples
{
    class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("id: {0}, name: {1}", Id, Name);
        }
    }

    class Program
    {
        static async Task Sample1()
        {
            var workflow = FluentFlow.Input<string>()
               .Sequence()
               .Then(_ => _.Split(','))
               .Then(FluentFlow.Input<IEnumerable<string>>()
                   .Fork()
                   .ForEach(FluentFlow.Input<string>()
                       .Parallel()
                       .Do(int.Parse)
                       .Do(_ => _)
                       .Join((id, name) => new User { Id = id, Name = name }))
                   .Join())
               .End()
               .Build("load multiple users");

            var users = await workflow.Start("1,2,3");

            foreach (var user in users)
            {
                Console.WriteLine(user);
            }
        }

        static async Task Sample2()
        {
            var workflow = FluentFlow.Input<string>()
                .Sequence()
                .Then(_ => _.ToCharArray())
                .Then(FluentFlow.Input<IEnumerable<char>>()
                    .Fork()
                    .ForEach(async _ => {await Task.Delay(100); return 1;})
                    .Join())
                .Then(array => array.Count())
                .End()
                .Build("count string length");

            var count = await workflow.Start("abcdefgh");
            Console.WriteLine(count);
        }

        static async Task Sample3()
        {
            var workflow = FluentFlow.Input<string>()
                .Sequence()
                .Then(_ => _.Split('+'))
                .Then(FluentFlow.Input<IEnumerable<string>>()
                    .Parallel(2)
                    .Do(items => int.Parse(items.First()))
                    .Do(items => int.Parse(items.Last()))
                    .Join((x, y) => x + y))
                .End()
                .Build("x + y = ?");

            var result = await workflow.Start("1+2");
            Console.WriteLine(result);
        }

        static async Task Sample4()
        {
            var workflow = FluentFlow.Input<string>()
                .Sequence()
                .Then(int.Parse)
                .Then(_ => new User {Id = 1, Name = "mike"})
                .End()
                .Build("load a single user");

            var user = await workflow.Start("1");
            Console.WriteLine(user);
        }

        static void Main(string[] args)
        {
            var all = new[]
            {
                Sample1(),
                Sample2(),
                Sample3(),
                Sample4()
            };

            Task.WaitAll(all);

            Console.ReadLine();
        }
    }
}
