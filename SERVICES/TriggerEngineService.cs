using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.SERVICES
{
    public class TriggerEngineService : ITriggerEngineService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<TriggerEngineService> _logger;
        private readonly IWorkflowExecutionService _workflowExecutionService;
        public TriggerEngineService(AppDbContext dbContext, ILogger<TriggerEngineService> logger, IWorkflowExecutionService workflowExecutionService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _workflowExecutionService = workflowExecutionService;
        }

        public async Task ExecuteTriggerAsync(TriggerEvent triggerEvent)
        {
            try
            {
                var workflows = await _dbContext.AutomationRules
                    .Include(w => w.Steps)
                    .Where(w => w.Trigger == triggerEvent.TriggerName && w.IsActive &&
                                (string.IsNullOrEmpty(triggerEvent.UserId) || w.UserID == triggerEvent.UserId))
                    .ToListAsync();

                foreach (var workflow in workflows)
                {
                    await _workflowExecutionService.ExecuteRuleAsync(workflow.ID, triggerEvent.Payload);
                }
                _logger.LogInformation("Trigger Executed Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }
    }
}
