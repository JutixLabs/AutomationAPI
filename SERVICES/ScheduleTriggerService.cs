using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;

namespace AutomationAPI.SERVICES
{
    public class ScheduleTriggerService
    {
        private readonly ITriggerEngineService _triggerEngine;
        private readonly ILogger<ScheduleTriggerService> _logger;
        public ScheduleTriggerService(ITriggerEngineService triggerEngine, ILogger<ScheduleTriggerService> logger)
        {
            _triggerEngine = triggerEngine;
            _logger = logger;
        }

        public async Task FireDailyTrigger()
        {
            try
            {
                await _triggerEngine.ExecuteTriggerAsync(
                        new TriggerEvent
                        {
                            TriggerName = "schedule.every_day",
                            UserId = "system",
                            Payload = new Dictionary<string, object>
                            {
                                { "execution.time", DateTime.UtcNow.ToString("o") }
                            }
                        });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }

        public async Task FireHourlyTrigger()
        {
            try
            {
                await _triggerEngine.ExecuteTriggerAsync(
                        new TriggerEvent
                        {
                            TriggerName = "schedule.every_hour",
                            UserId = "system",
                            Payload = new Dictionary<string, object>()
                        });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }
    }
}
