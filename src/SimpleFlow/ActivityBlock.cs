using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFlow.Core
{
    class ActivityBlock : WorkflowBlock
    {
        //todo: string Name?
        public ActivityBlock(Delegate method)
        {
            if (method == null) throw new ArgumentNullException("method");
            Method = method;

            InputTypes = method.Method.GetParameters().Select(_ => _.ParameterType).ToArray();

            var outputType = method.Method.ReturnType;

            if (typeof (Task).IsAssignableFrom(outputType))
            {
                IsAsync = true;
                if (outputType.IsGenericType)
                {
                    OutputType = outputType.GetGenericArguments().Single();
                }
                else
                {
                    OutputType = typeof (void); //todo: use null?
                }
            }
            else
            {
                OutputType = outputType;
            }
        }

        public Delegate Method { get; private set; }
        public bool IsAsync { get; private set; }

        public override WorkflowType Type
        {
            get { return WorkflowType.Activity; }
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}