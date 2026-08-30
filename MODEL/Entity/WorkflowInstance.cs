namespace AutomationAPI.MODEL.Entity
{
    public class WorkflowInstance
    {
        public int Id { get; set; }
        public int RuleId { get; set; }
        public int CurrentStepId { get; set; }
        public string Status { get; set; }
        public string PayloadJson { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? LastExecutedAt { get; set; }
    }
}
