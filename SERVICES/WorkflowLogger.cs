using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;

namespace AutomationAPI.SERVICES
{
    public class WorkflowLogger : IWorkflowLogger
    {
        private readonly ILogger<WorkflowLogger> _logger;   
        private readonly AppDbContext _dbContext;
        public WorkflowLogger(ILogger<WorkflowLogger> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task LogAsync(int ruleId, int stepId, string status, string message, int attempt)
        {
            try
            {
                var log = new WorkflowExecutionLog
                {
                    RuleId = ruleId,
                    RuleStepId = stepId,
                    Status = status,
                    Message = message,
                    Attempt = attempt,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.WorkflowExecutionLogs.Add(log);

                _logger.LogInformation($"[INFO] Logged workflow execution: RuleId={ruleId}, StepId={stepId}, Status={status}, Attempt={attempt}");
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Failed to log workflow execution: {ex.Message}");
            }
        }
    }
}
