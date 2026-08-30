using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IWorkflowInstanceService
    {
        Task<WorkflowInstance> CreateAsync(int ruleId, int stepId, Dictionary<string, object> payload);
        Task UpdateStepAsync(int instanceId, int stepId, string status);
        Task CompleteAsync(int instanceId);
        Task FailAsync(int instanceId);
    }
}
