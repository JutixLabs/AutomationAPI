using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IExecutionLogService
    {
        Task<List<ExecutionLog>> GetExecutionLogAsync();
        Task<List<ExecutionLog>> DeleteAllLogsAsync();
    }
}
