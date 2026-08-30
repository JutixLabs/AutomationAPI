using Newtonsoft.Json;

namespace AutomationAPI.MODEL.DTO
{
    public class NotionTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("workspace_id")]
        public string WorkspaceId { get; set; }

        [JsonProperty("workspace_name")]
        public string WorkspaceName { get; set; }

        [JsonProperty("bot_id")]
        public string BotId { get; set; }
    }
}