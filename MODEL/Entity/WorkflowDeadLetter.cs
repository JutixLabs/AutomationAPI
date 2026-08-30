namespace AutomationAPI.MODEL.Entity
{
    public class WorkflowDeadLetter
    {
        public int Id { get; set; }
        public int RuleId { get; set; }
        public int RuleStepId { get; set; }
        public string PayloadJson { get; set; }
        public string ErrorMessage { get; set; }
        public int Attempts { get; set; }
        public bool Resolved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
