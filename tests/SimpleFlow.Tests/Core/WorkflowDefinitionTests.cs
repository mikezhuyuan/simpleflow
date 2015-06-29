using System;
using System.Collections.Generic;
using System.Linq;
using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class WorkflowDefinitionTests
    {
        [Fact]
        public void ActivityConstruction()
        {
            Func<int, int, string> add = (x, y) => string.Format("{0} + {1} = {2}", x, y, x + y);

            var activity = new ActivityBlock(add);

            Assert.Equal(WorkflowType.Activity, activity.Type);
            Assert.Equal(new[] {typeof (int), typeof (int)}, activity.InputTypes);
            Assert.Equal(typeof (string), activity.OutputType);

            // test method with signature void(void)
            activity = new ActivityBlock(new Action(delegate { }));
            Assert.False(activity.InputTypes.Any());
            Assert.Equal(typeof (void), activity.OutputType);
        }

        [Fact]
        public void MapConstruction()
        {
            Func<int, int> pow = x => x*x;

            var map = new ForkBlock(new ActivityBlock(pow), 2);

            Assert.Equal(WorkflowType.Fork, map.Type);
            Assert.Equal(2, map.MaxWorkers);
            Assert.Equal(new[] {typeof (int)}, map.InputTypes);
            Assert.Equal(typeof (IEnumerable<int>), map.OutputType);

            // test method with signature void(void)
            map = new ForkBlock(new ActivityBlock(new Action(delegate { })), 1);
            Assert.Equal(typeof (void), map.OutputType);
        }

        [Fact]
        public void SequenceConstruction()
        {
            Func<string, int> parse = int.Parse;
            Func<int, string> stringify = x => x.ToString();

            var seq = new SequenceBlock(new[] {new ActivityBlock(parse), new ActivityBlock(stringify)});

            Assert.Equal(WorkflowType.Sequence, seq.Type);
            Assert.Equal(new[] {typeof (string)}, seq.InputTypes);
            Assert.Equal(typeof (string), seq.OutputType);

            // test method with signature void(void)
            seq = new SequenceBlock(new[] {new ActivityBlock(new Action(delegate { }))});
            Assert.Equal(new Type[0], seq.InputTypes);
            Assert.Equal(typeof (void), seq.OutputType);
        }

        [Fact]
        public void PallaralConstruction()
        {
            Func<string, int> parse = int.Parse;
            Func<string, int> count = s => s.Length;

            var parallel = new ParallelBlock(new[] {new ActivityBlock(parse), new ActivityBlock(count)}, 1);

            Assert.Equal(WorkflowType.Parallel, parallel.Type);
            Assert.Equal(new[] {typeof (string)}, parallel.InputTypes);
            Assert.Equal(typeof (IEnumerable<object>), parallel.OutputType);

            // test method with signature void(void)
            parallel = new ParallelBlock(new[] {new ActivityBlock(new Action(delegate { }))}, 1);
            Assert.Equal(new Type[0], parallel.InputTypes);
            Assert.Equal(typeof (void), parallel.OutputType);
        }
    }
}