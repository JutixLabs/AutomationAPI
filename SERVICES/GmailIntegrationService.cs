using AutomationAPI.MODEL.DTO;
using AutomationAPI.SERVICES.Persistence;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace AutomationAPI.SERVICES
{
    public class GmailIntegrationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GmailIntegrationService> _logger;
        public GmailIntegrationService(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, ILogger<GmailIntegrationService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task SendEmail(string accessToken, string to, string subject, string body)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
                var credential = GoogleCredential.FromAccessToken(accessToken);

                var service =
                    new Google.Apis.Gmail.v1.GmailService(
                        new BaseClientService.Initializer
                        {
                            HttpClientInitializer =
                                credential,
                            ApplicationName =
                                "Automation Platform"
                        }
                    );

                var rawMessage =
                    $"To: {to}\r\n" +
                    $"Subject: {subject}\r\n\r\n" +
                    $"{body}";

                var message =
                    new Message
                    {
                        Raw = Base64UrlEncode(rawMessage)
                    };

                await service.Users.Messages.Send(
                    message,
                    "me"
                ).ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }

        public async Task<GoogleProfileDto> GetProfileAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            accessToken);

                var response =
                    await _httpClient.GetAsync(
                        "https://www.googleapis.com/oauth2/v2/userinfo");

                response.EnsureSuccessStatusCode();

                var json =
                    await response.Content
                        .ReadAsStringAsync();

                return JsonConvert.DeserializeObject<
                    GoogleProfileDto>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task<List<GmailMessageDTO>> GetLatestEmailAsync(string accessToken)
        {
            try
            {
                var gmailService = CreateService(accessToken);
                var request = gmailService.Users.Messages.List("me");

                request.MaxResults = 10;

                var response = await request.ExecuteAsync();

                var emails = new List<GmailMessageDTO>();

                if (response.Messages == null)
                    return emails;

                foreach (var msg in response.Messages)
                {
                    var message = await gmailService.Users.Messages.Get("me", msg.Id).ExecuteAsync();

                    var headers = message.Payload.Headers;

                    emails.Add(new GmailMessageDTO
                    {
                        Id = message.Id,
                        Subject = headers.FirstOrDefault(h => h.Name == "Subject")?.Value,
                        From = headers.FirstOrDefault(h => h.Name == "From")?.Value,
                        Body = ""
                    });
                }

                return emails;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        private string Base64UrlEncode(string input)
        {
            return Convert.ToBase64String(
                    System.Text.Encoding.UTF8
                        .GetBytes(input))
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        private Google.Apis.Gmail.v1.GmailService CreateService(string accessToken)
        {
            var credential = GoogleCredential.FromAccessToken(accessToken);
            return new Google.Apis.Gmail.v1.GmailService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "AutomationsAPI"
                }
            );
        }
    }
}
