using Newtonsoft.Json;

namespace AutomationAPI.MODEL.DTO
{
    public class SlackEventRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("challenge")]
        public string Challenge { get; set; }

        [JsonProperty("event")]
        public SlackEvent Event { get; set; }
    }

    public class SlackEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }
    }
}
