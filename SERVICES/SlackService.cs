using AutomationAPI.MODEL.DTO;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace AutomationAPI.SERVICES
{
    public class SlackService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SlackService> _logger;
        public SlackService(IHttpClientFactory httpClientFactory, ILogger<SlackService> logger, HttpClient httpClient)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task SendMessage(string accessToken, string channel, string text)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers
                        .AuthenticationHeaderValue(
                            "Bearer",
                            accessToken);

                var payload = new
                {
                    channel,
                    text
                };

                var content =
                    new StringContent(
                        Newtonsoft.Json.JsonConvert
                            .SerializeObject(payload),
                        System.Text.Encoding.UTF8,
                        "application/json");

                var response = await client.PostAsync(
                    "https://slack.com/api/chat.postMessage",
                    content);
                var json = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(json);

                if (result.ok != true)
                {
                    _logger.LogError("[ERROR] Slack SendMessage failed: {Error}", (string)result.error);
                    throw new Exception(
                        $"Error sending message to Slack: {result.error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }

        public async Task<SlackProfileDto> GetProfileAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            accessToken);

                var response =
                    await _httpClient.GetAsync(
                        "https://slack.com/api/users.identity");

                response.EnsureSuccessStatusCode();

                var json =
                    await response.Content
                        .ReadAsStringAsync();

                return JsonConvert.DeserializeObject<
                    SlackProfileDto>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }
    }
}
