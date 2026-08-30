using Newtonsoft.Json;

namespace AutomationAPI.MODEL.DTO
{
    public class DiscordProfileDto
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        [JsonProperty("global_name")]
        public string GlobalName { get; set; }
    }
}
