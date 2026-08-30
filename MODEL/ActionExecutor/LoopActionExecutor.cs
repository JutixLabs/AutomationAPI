using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;

namespace AutomationAPI.MODEL.ActionExecutor
{
    public class LoopActionExecutor : IActionExecutor
    {
        private readonly ILogger<LoopActionExecutor> _logger;
        public LoopActionExecutor(ILogger<LoopActionExecutor> logger)
        {
            _logger = logger;
        }
        public string Provider => "loop";

        public async Task ExecuteAsync(string userId, WorkFlowStep step, Dictionary<string, object> payload)
        {
            try
            {
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }
    }
}
