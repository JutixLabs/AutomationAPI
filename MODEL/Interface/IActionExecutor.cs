using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IActionExecutor
    {
        string Provider { get; }
        Task ExecuteAsync(string userId, WorkFlowStep step, Dictionary<string, object> payload);
    }
}
