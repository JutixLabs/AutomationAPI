namespace AutomationAPI.MODEL.Interface
{
    public interface IWorkflowExecutionService
    {
        Task ExecuteRuleAsync(int ruleId, Dictionary<string, object> payload);
    }
}
