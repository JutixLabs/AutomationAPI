namespace AutomationAPI.MODEL.Entity
{
    public class WorkflowExecution
    {
        public int Id { get; set; }
        public int RuleId { get; set; }
        public string Payload { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    }
}
