using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.MODEL.Interface
{
    public interface ITriggerEngineService
    {
        Task ExecuteTriggerAsync(TriggerEvent triggerEvent);
    }
}
