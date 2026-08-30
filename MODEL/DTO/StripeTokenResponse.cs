using Newtonsoft.Json;

namespace AutomationAPI.MODEL.DTO
{
    public class StripeTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("stripe_user_id")]
        public string StripeUserId { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("livemode")]
        public bool Livemode { get; set; }
    }
}
