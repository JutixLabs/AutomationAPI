using Newtonsoft.Json;

namespace AutomationAPI.MODEL.DTO
{
    public class TrelloMemberDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        [JsonProperty("fullName")]
        public string FullName { get; set; }
    }
}
