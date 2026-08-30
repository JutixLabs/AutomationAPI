using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;

namespace AutomationAPI.MODEL.ActionExecutor
{
    public class DelayActionExecutor : IActionExecutor
    {
        private readonly ILogger<DelayActionExecutor> _logger;
        public DelayActionExecutor(ILogger<DelayActionExecutor> logger)
        {
            _logger = logger;
        }
        public string Provider => "delay";

        public async Task ExecuteAsync(string userId, WorkFlowStep step, Dictionary<string, object> payload)
        {
            // Delay executor itself does nothing.
            // Workflow engine handles scheduling.

            await Task.CompletedTask;
        }

        public  TimeSpan GetDelay(WorkFlowStep step)
        {
            try
            {
                var amount = step.DelayAmount ?? 0;

                return step.DelayUnit?.ToLower() switch
                {
                    "minutes" => TimeSpan.FromMinutes(amount),

                    "hours" => TimeSpan.FromHours(amount),

                    "days" => TimeSpan.FromDays(amount),

                    _ => TimeSpan.Zero
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }
    }
}
