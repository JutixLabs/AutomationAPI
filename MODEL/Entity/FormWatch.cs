namespace AutomationAPI.MODEL.Entity
{
    public class FormWatch
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int RuleId { get; set; }
        public string SpreadSheetId { get; set; }
        public string SheetName { get; set; }
        public int LastProcessedRow { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
