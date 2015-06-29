using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFlow.Core
{
    //todo: add logging facility
    public static class Helpers
    {
        public static object GetResult(this Task task)
        {
            if (!task.GetType().IsGenericType)
                return null;

            return task.GetType().GetProperty("Result").GetValue(task);
        }

        public static bool IsCollection(this Type type)
        {
            return type != typeof (string) && typeof (IEnumerable).IsAssignableFrom(type);
        }

        public static Type Unamplify(this Type type)
        {
            return type.IsArray ? type.GetElementType() : type.GetGenericArguments().Single();
        }
    }
}