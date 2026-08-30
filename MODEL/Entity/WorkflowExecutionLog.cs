namespace AutomationAPI.MODEL.Entity
{
    public class WorkflowExecutionLog
    {
        public int Id { get; set; }

        public int RuleId { get; set; }

        public int RuleStepId { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public int Attempt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
