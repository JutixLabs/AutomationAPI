namespace AutomationAPI.MODEL.Entity
{
    public class AutomationRule
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string Trigger { get; set; }
        public List<WorkFlowStep> Steps { get; set; } = new();

        public bool IsWorkflow { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public int? FolderId { get; set; }
        public Folder Folder { get; set; }

        public string WebhookKey { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
