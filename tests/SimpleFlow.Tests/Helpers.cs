using System;
using System.Collections.Generic;
using SimpleFlow.Core;

namespace SimpleFlow.Tests
{
    static class Helpers
    {
        public static readonly DateTime Now = DateTime.Now;

        public static ActivityBlock BuildActivity()
        {
            Func<int, int> identity = x => x;
            return new ActivityBlock(identity);
        }

        public static ForkBlock BuildFork(int maxWorkers = 1)
        {
            return new ForkBlock(BuildActivity(), maxWorkers);
        }

        public static int Integer()
        {
            return new Random().Next(0, 1000);
        }

        public static IEnumerable<int> Integers(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return Integer();
            }
        }

        public static string String()
        {
            return new Random().Next(0, 1000).ToString();
        }

        public static Func<Exception, TOutput> ExceptionHandler<TOutput>()
        {
            return ex => default(TOutput);
        }
    }
}