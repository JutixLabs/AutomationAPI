namespace AutomationAPI.MODEL.DTO
{
    public class WorkflowExecutionContext
    {
        public TriggerEvent TriggerEvent { get; set; }
        public Dictionary<string, object> Variables { get; set; }
    } 
}
