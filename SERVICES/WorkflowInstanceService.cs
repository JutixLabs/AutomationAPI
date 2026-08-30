using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using System.Text.Json;

namespace AutomationAPI.SERVICES
{
    public class WorkflowInstanceService : IWorkflowInstanceService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<WorkflowInstanceService> _logger;
        public WorkflowInstanceService(AppDbContext dbContext, ILogger<WorkflowInstanceService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task CompleteAsync(int instanceId)
        {
            try
            {
                var instance = await _dbContext.WorkflowInstances.FindAsync(instanceId);

                if (instance == null) 
                    throw new Exception($"Instance with Id:{instanceId} not found.");

                instance.Status = "COMPLETED";
                instance.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation("Workflow Instance Completed.");
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }

        public async Task<WorkflowInstance> CreateAsync(int ruleId, int stepId, Dictionary<string, object> payload)
        {
            try
            {
                var instance = new WorkflowInstance
                {
                    RuleId = ruleId,
                    CurrentStepId = stepId,
                    Status = "RUNNING",
                    PayloadJson = JsonSerializer.Serialize(payload),
                    StartedAt = DateTime.UtcNow,
                    LastExecutedAt = DateTime.UtcNow,
                };

                _dbContext.WorkflowInstances.Add(instance);

                _logger.LogInformation("Workflow Instance Created Succesfuly.");
                await _dbContext.SaveChangesAsync();

                return instance;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task FailAsync(int instanceId)
        {
            try
            {
                var instance = await _dbContext.WorkflowInstances.FindAsync(instanceId);

                if (instance == null)
                    throw new Exception($"Instance with Id:{instanceId} not found.");

                instance.Status = "FAILED";

                _logger.LogWarning("Workflow Instance Failed.");
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }

        public async Task UpdateStepAsync(int instanceId, int stepId, string status)
        {
            try
            {
                var instance = await _dbContext.WorkflowInstances.FindAsync(instanceId);

                if (instance == null)
                    throw new Exception($"Instance with Id:{instanceId} not found.");

                instance.CurrentStepId = stepId;
                instance.Status = status;
                instance.LastExecutedAt = DateTime.UtcNow;

                _logger.LogInformation("Workflow Instance Updated Successfuly.");
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }
    }
}
