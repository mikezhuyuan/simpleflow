namespace SimpleFlow.Core
{
    interface IStateMachine
    {
        void Transit(WorkItem workItem, IEngine engine);
    }
}