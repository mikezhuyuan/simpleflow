namespace SimpleFlow.Core
{
    public class WorkflowJob
    {
        public int Id { get; set; }
        public string Status { get; set; } //todo: enum
        public string WorkflowName { get; set; }
        public string Signature { get; set; }
    }
}