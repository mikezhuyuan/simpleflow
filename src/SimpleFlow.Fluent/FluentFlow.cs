namespace SimpleFlow.Fluent
{
    //todo: combine with Input<TInput>
    public static class FluentFlow //todo: fluent flow is a good candidate for the whole project
    {
        public static Input<TInput> Input<TInput>()
        {
            return new Input<TInput>();
        }
    }
}