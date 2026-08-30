namespace AutomationAPI.MODEL.Entity
{
    public class ExecutionLog
    {
        public int ID { get; set; }
        public string UserId { get; set; }
        public string Trigger { get; set; }
        public string Action { get; set; }
        public string Target { get; set; }
        public string Status { get; set; }
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }
}
