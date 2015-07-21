using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Core;
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

    class Response<TResult>
    {
        public TResult Result;
        public bool Success;
    }

    class Program
    {
        static async Task Sample1()
        {
            Func<string, IEnumerable<string>> split = _ => _.Split(',');
            Func<string, int> parse = int.Parse;

            var workflow = FluentFlow
                .Sequence()
                .Begin(split)
                .Then(FluentFlow
                    .Fork()
                    .ForEach(FluentFlow
                        .Sequence()
                        .Begin(FluentFlow
                            .Parallel()
                            .Do(parse)
                            .Do(_ => _)
                            .Join())
                        .Then(_ => new User {Id = _.Item1, Name = _.Item2})
                        .End()))
                .End()
                .Build("load multiple users");

            var users = await WorkflowRunner.Run(workflow, "1,2,3");

            foreach (var user in users)
            {
                Console.WriteLine(user);
            }
        }

        static async Task Sample2()
        {
            Func<string, char[]> toChars = _ => _.ToCharArray();
            Func<char, Task<int>> one = async _ =>
            {
                await Task.Delay(100);
                return 1;
            };

            var workflow = FluentFlow
                .Sequence()
                .Begin(toChars)
                .Then(FluentFlow
                    .Fork()
                    .ForEach(one))
                .Then(array => array.Count())
                .End()
                .Build("count string length");

            var count = await WorkflowRunner.Run(workflow, "abcdefgh");
            Console.WriteLine(count);
        }

        static async Task Sample3()
        {
            Func<string, string[]> split = _ => _.Split('+');
            Func<IEnumerable<string>, int> parseFirst = _ => int.Parse(_.First());
            Func<IEnumerable<string>, int> parseLast = _ => int.Parse(_.Last());

            var workflow = FluentFlow
                .Sequence()
                .Begin(split)
                .Then(
                    FluentFlow
                        .Parallel(2)
                        .Do(parseFirst)
                        .Do(parseLast)
                        .Join())
                .Then(_ => _.Item1 + _.Item2)
                .End()
                .Build("x + y = ?");

            var result = await WorkflowRunner.Run(workflow, "1+2");
            Console.WriteLine(result);
        }

        static async Task Sample4()
        {
            Func<string, int> parse = int.Parse;

            var workflow = FluentFlow
                .Sequence()
                .Begin(parse)
                .Then(_ => new User {Id = 1, Name = "mike"})
                .End()
                .Build("load a single user");

            var user = await WorkflowRunner.Run(workflow, "1");
            Console.WriteLine(user);
        }

        static async Task Sample5()
        {
            Func<int, Response<int>> divide = i => new Response<int> {Result = i/i};

            var workflow =
                FluentFlow.Activity(divide)
                    .Catch(ex => new Response<int> {Result = -1, Success = false})
                    .Build("divide");

            var r = await WorkflowRunner.Run(workflow, 0);
            Console.WriteLine("success: {0}, result: {1}", r.Success, r.Result);
        }

        static async Task Sample6()
        {
            Func<string, Task<int>> parseAsync = _ => Task.FromResult(int.Parse(_));

            var workflow = FluentFlow.Sequence()
                .Begin(FluentFlow.Fork(2)
                    .ForEach(parseAsync))
                .Then(_ => _.Sum())
                .End()
                .Catch(_ => -1)
                .Build("sum");

            var r = await WorkflowRunner.Run(workflow, new[] {"1", "~", "3", "4", "5"});

            Console.WriteLine(r);
        }

        static async Task Sample7()
        {
            var rand = new Random();
            Func<int, bool> retry = _ =>
            {
                if (rand.Next(0, 10) < 5)
                    throw new Exception();

                return true;
            };

            var workflow = FluentFlow.Sequence()
                .Begin(FluentFlow.Fork(5)
                                .ForEach(FluentFlow.Activity(retry).Catch(_ => false)))
                .Then(_ => _.Count(_1 => _1)/(double) _.Count())
                .End()
                .Build("sum");

            var r = await WorkflowRunner.Run(workflow, Enumerable.Range(1, 1000));

            Console.WriteLine(r);
        }

        static async Task Sample8()
        {
            var rand = new Random();
            Func<int, IEnumerable<int>> repeat = _ => Enumerable.Repeat(1, _);
            Func<int, Task<int>> one = async _ =>
            {
                await Task.Delay(rand.Next(1, 200));
                return 1;
            };
            var workflow = FluentFlow.Sequence()
                .Begin(repeat)
                .Then(FluentFlow.Fork(int.MaxValue).ForEach(one))
                .Then(_ => _.Sum())
                .End()
                .Build("sum");

            var r = await WorkflowRunner.Run(workflow, 1000);

            Console.WriteLine(r);
        }

        static async Task Sample9()
        {
            var i = -1;
            var workflow = FluentFlow.Activity<int, int>(x =>
            {
                if (i++ < x)
                    throw new Exception();

                return i;
            }).Retry(5).Build("retry");

            var r = await WorkflowRunner.Run(workflow, 1);

            Console.WriteLine(r);
        }

        static void Main()
        {
            var all = new[]
            {
                //Sample1(),
                //Sample2(),
                //Sample3(),
                //Sample4(),
                //Sample5(),
                //Sample6(),
                Sample7()
                //Sample8(),
                //Sample9()
            };

            Task.WaitAll(all);

            Console.ReadLine();
        }
    }
}