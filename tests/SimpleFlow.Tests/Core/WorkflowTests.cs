using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class WorkflowTests
    {
        [Fact]
        public async Task TestActivity()
        {
            Func<int, int> pow = x => x*x;

            var activity = new ActivityBlock(pow);

            var definition = new WorkflowDefinition("test", activity);
            var output = await Workflow.Start<int, int>(2, definition);

            Assert.Equal(4, output);
        }

        [Fact]
        public async Task TestAsync()
        {
            Func<int, Task<int>> identity = async x =>
            {
                await Task.Delay(100);
                return x;
            };

            var activity = new ActivityBlock(identity);

            var definition = new WorkflowDefinition("test", activity);
            var output = await Workflow.Start<int, int>(1, definition);

            Assert.Equal(1, output);
        }

        [Fact]
        public async Task TestSequence()
        {
            Func<int, int> increment = x => x + 1;

            var a1 = new ActivityBlock(increment);
            var a2 = new ActivityBlock(increment);

            var seq = new SequenceBlock(new[] {a1, a2});

            var definition = new WorkflowDefinition("test", seq);

            var output = await Workflow.Start<int, int>(0, definition);

            Assert.Equal(2, output);
        }

        [Fact]
        public async Task TestVeryLongSequence()
        {
            Func<int, int> increment = x => x + 1;

            var steps = new List<ActivityBlock>();
            var count = 1000;
            for (var i = 0; i < count; i++)
            {
                steps.Add(new ActivityBlock(increment));
            }

            var seq = new SequenceBlock(steps);

            var definition = new WorkflowDefinition("test", seq);

            var output = await Workflow.Start<int, int>(0, definition);

            Assert.Equal(count, output);
        }

        [Fact]
        public async Task TestFork()
        {
            Func<int, int> twice = x => 2*x;

            var activity = new ActivityBlock(twice);
            var fork = new ForkBlock(activity, 2);

            var definition = new WorkflowDefinition("test", fork);
            var output = await Workflow.Start<int[], IEnumerable<int>>(new[] {1, 2, 3}, definition);
            //int[] would work but not recommended

            Assert.Equal(new[] {2, 4, 6}, output);
        }

        [Fact]
        public async Task TestLargeChunckOfFork()
        {
            Func<int, int> identity = x => x;

            var activity = new ActivityBlock(identity);
            var fork = new ForkBlock(activity, 1);

            var definition = new WorkflowDefinition("test", fork);
            var input = Enumerable.Repeat(1, 1000).ToArray();
            var output = await Workflow.Start<IEnumerable<int>, IEnumerable<int>>(input, definition);
            //int[] would work but not recommended

            Assert.Equal(input, output);
        }

        [Fact]
        public async Task TestChainedFork()
        {
            Func<int, int> identity = x => x;

            var activity = new ActivityBlock(identity);
            var fork = new ForkBlock(new ForkBlock(new ForkBlock(activity, 1), 1), 1);

            var definition = new WorkflowDefinition("test", fork);
            var output =
                await
                    Workflow.Start<int[][][], IEnumerable<IEnumerable<IEnumerable<int>>>>(new[] {new[] {new[] {1}}},
                        definition);

            Assert.Equal(new[] {1}, output.SelectMany(_ => _).SelectMany(_ => _));
        }

        [Fact]
        public async Task TestParallel()
        {
            Func<int, string> stringify = x => x.ToString();
            Func<int, int> identity = x => x;

            var parallel = new ParallelBlock(new[] {new ActivityBlock(stringify), new ActivityBlock(identity)}, 1);
            var definition = new WorkflowDefinition("test", parallel);
            var output = (await Workflow.Start<int, IEnumerable<object>>(1, definition)).ToArray();

            Assert.Equal(2, output.Length);
            Assert.Equal("1", output[0]);
            Assert.Equal(1, output[1]);
        }

        [Fact]
        public async Task TestIntegration1()
        {
            Func<string, string[]> split = s => s.Split(',');
            Func<string, int> parse = int.Parse;
            Func<int[], int> sum = nums => nums.Sum();

            var root = new SequenceBlock(new WorkflowBlock[]
            {
                new ActivityBlock(split),
                new ForkBlock(new ActivityBlock(parse), 2),
                new ActivityBlock(sum)
            });

            var definition = new WorkflowDefinition("test", root);
            var output = await Workflow.Start<string, int>("1,2,3,4,5,6,7,8,9,10", definition);

            Assert.Equal(55, output);
        }

        [Fact]
        public async Task TestIntegration2()
        {
            Func<string, Task<string[]>> split = s => Task.FromResult(s.Split(','));
            Func<string, Task<int>> convert = s => Task.FromResult(int.Parse(s));
            Func<int[], Task<int>> sum = nums => Task.FromResult(nums.Sum());

            var root = new SequenceBlock(new WorkflowBlock[]
            {
                new ActivityBlock(split),
                new ForkBlock(new ActivityBlock(convert), 2),
                new ActivityBlock(sum)
            });

            var definition = new WorkflowDefinition("test", root);
            var output = await Workflow.Start<string, int>("1,2,3,4,5,6,7,8,9,10", definition);

            Assert.Equal(55, output);
        }

        [Fact]
        public async Task TestIntegration3()
        {
            Func<string, int> parse = int.Parse;
            Func<int, int, int> add = (x, y) => x + y;

            var root = new SequenceBlock(new WorkflowBlock[]
            {
                new ParallelBlock(new[] {new ActivityBlock(parse), new ActivityBlock(parse)}, 2),
                new ActivityBlock(add)
            });

            var definition = new WorkflowDefinition("test", root);

            var output = await Workflow.Start<string, int>("1", definition);

            Assert.Equal(2, output);
        }

        [Fact]
        public async Task TestIntegration4()
        {
            Func<string, Task<string[]>> split = str => Task.FromResult(str.Split(','));
            Func<string, Task<int>> parse = i => Task.FromResult(int.Parse(i));
            Func<int, Task<User>> load = id => Task.FromResult(new User {Id = id});
            Func<int, Task<string>> name = id => Task.FromResult("n" + id);
            Func<User, string, Task<User>> projection = async (user, s) =>
            {
                await Task.Delay(100);
                user.Name = s;
                return user;
            };

            var root = new SequenceBlock(new WorkflowBlock[]
            {
                new ActivityBlock(split),
                new ForkBlock(
                    new SequenceBlock(
                        new WorkflowBlock[]
                        {
                            new ActivityBlock(parse),
                            new ParallelBlock(new[]
                            {
                                new ActivityBlock(load),
                                new ActivityBlock(name)
                            }, 1),
                            new ActivityBlock(projection)
                        }), 1)
            });

            var definition = new WorkflowDefinition("test", root);
            var output = await Workflow.Start<string, IEnumerable<User>>("1,2,3", definition);

            Assert.Equal(
                new[] {new User {Id = 1, Name = "n1"}, new User {Id = 2, Name = "n2"}, new User {Id = 3, Name = "n3"}},
                output);
        }

        class User
        {
            public int Id;
            public string Name;

            public override bool Equals(object obj)
            {
                var that = (User) obj;

                return Id == that.Id && Name == that.Name;
            }
        }
    }
}