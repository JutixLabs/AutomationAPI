namespace AutomationAPI.MODEL.Interface
{
    public interface IDeadLetterService
    {
        Task SaveAsync(int ruleId, int stepId, Dictionary<string, object> payload, string error, int attempts);
    }
}
