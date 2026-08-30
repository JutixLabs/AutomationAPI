namespace AutomationAPI.MODEL.DTO
{
    public class RequestWorkFlowSteps
    {
        public int Order { get; set; }
        public string Action { get; set; }
        public string Target { get; set; }

        // JSON blob for actions needing more than one configured field
        // (e.g. Discord send_message needs serverId + channelId + message).
        // Frontend should JSON.stringify the extra field values here.
        public string ConfigurationJson { get; set; }
    }
}
