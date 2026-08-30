using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IAutomationService
    {
        Task<object> GetAllRulesAsync();
        Task<AutomationRule> CreatRuleAsync(CreateWorkFlow rule);
        Task<List<ExecutionLog>> ExecuteTrigger(string trigger);
        Task<bool> DeleteByIdAsync(int id);
        Task<List<AutomationRule>> DeleteAllRulesAsync();
        Task<bool> ToggleRuleAsync(int id);
    }
}
