namespace AutomationAPI.MODEL.DTO
{
    public class CreateWorkFlow
    {
        public string Trigger { get; set; } 
        public int? FolderId { get; set; }
        public List<RequestWorkFlowSteps> Steps { get; set; }
    }
}
