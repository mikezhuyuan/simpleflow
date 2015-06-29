using SimpleFlow.Core;
using Xunit;

namespace SimpleFlow.Tests.Core
{
    public class WorkflowPathNavigatorTests
    {
        [Fact]
        public void Test()
        {
            var a1 = Helpers.BuildActivity();
            var a2 = Helpers.BuildActivity();
            var a3 = Helpers.BuildActivity();
            var a4 = Helpers.BuildActivity();
            var a5 = Helpers.BuildActivity();
            var c1 = new SequenceBlock(new[] {a1});
            var f = new ForkBlock(a2, 1);
            var p = new ParallelBlock(new[] {a4, a5}, 1);
            var c2 = new SequenceBlock(new WorkflowBlock[] {f, c1, a3, p});
            var wf = new WorkflowDefinition("", c2);

            var navigator = new WorkflowPathNavigator(wf);

            Assert.Equal("Sequence[0].Sequence[1].Activity[0]", navigator.Path(a1));
            Assert.Equal("Sequence[0].Fork[0].Activity[0]", navigator.Path(a2));
            Assert.Equal("Sequence[0].Activity[2]", navigator.Path(a3));
            Assert.Equal("Sequence[0].Sequence[1]", navigator.Path(c1));
            Assert.Equal("Sequence[0].Fork[0]", navigator.Path(f));
            Assert.Equal("Sequence[0]", navigator.Path(c2));
            Assert.Equal("Sequence[0].Parallel[3]", navigator.Path(p));
            Assert.Equal("Sequence[0].Parallel[3].Activity[0]", navigator.Path(a4));
            Assert.Equal("Sequence[0].Parallel[3].Activity[1]", navigator.Path(a5));

            Assert.Equal(a1, navigator.Find("Sequence[0].Sequence[1].Activity[0]"));
            Assert.Equal(a2, navigator.Find("Sequence[0].Fork[0].Activity[0]"));
            Assert.Equal(a3, navigator.Find("Sequence[0].Activity[2]"));
            Assert.Equal(c1, navigator.Find("Sequence[0].Sequence[1]"));
            Assert.Equal(f, navigator.Find("Sequence[0].Fork[0]"));
            Assert.Equal(c2, navigator.Find("Sequence[0]"));

            Assert.Equal(p, navigator.Find("Sequence[0].Parallel[3]"));
            Assert.Equal(a4, navigator.Find("Sequence[0].Parallel[3].Activity[0]"));
            Assert.Equal(a5, navigator.Find("Sequence[0].Parallel[3].Activity[1]"));
        }
    }
}