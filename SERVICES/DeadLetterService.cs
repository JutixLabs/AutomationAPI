using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using System.Text.Json;

namespace AutomationAPI.SERVICES
{
    public class DeadLetterService : IDeadLetterService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DeadLetterService> _logger;
        public DeadLetterService(AppDbContext dbContext, ILogger<DeadLetterService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task SaveAsync(int ruleId, int stepId, Dictionary<string, object> payload, string error, int attempts)
        {
            try
            {
                var deadLetter = new WorkflowDeadLetter
                {
                    RuleId = ruleId,
                    RuleStepId = stepId,
                    PayloadJson = JsonSerializer.Serialize(payload),
                    ErrorMessage = error,
                    Attempts = attempts,
                    Resolved = false,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.WorkflowDeadLetters.Add(deadLetter);

                _logger.LogInformation($"[INFO] Saved to dead letter: RuleId={ruleId}, StepId={stepId}, Attempts={attempts}, Error={error}");
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Failed to save to dead letter: {ex.Message}");
            }
        }
    }
}
