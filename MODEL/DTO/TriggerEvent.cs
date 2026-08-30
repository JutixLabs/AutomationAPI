using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace AutomationAPI.MODEL.DTO
{
    public class TriggerEvent
    {
        public string TriggerName { get; set; }
        public string UserId { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }
}
