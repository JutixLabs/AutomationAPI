namespace AutomationAPI.MODEL.Interface
{
    public interface IWorkflowLogger
    {
        Task LogAsync(int ruleId, int stepId, string status, string message, int attempt);
    }
}
