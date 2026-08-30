using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.SERVICES.Persistence;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace AutomationAPI.SERVICES
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;
        public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger, IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<string> GetRepositories(string accessToken)
        {
            try
            {
                using var client =
                        new HttpClient();

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        accessToken);

                client.DefaultRequestHeaders.UserAgent
                    .ParseAdd("JutixAutomation");

                return await client.GetStringAsync(
                    "https://api.github.com/user/repos");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task<GitHubProfileDto> GetProfileAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();

                _httpClient.DefaultRequestHeaders.Add(
                    "Authorization",
                    $"Bearer {accessToken}");

                _httpClient.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "AutomationAPI");

                var response =
                    await _httpClient.GetAsync(
                        "https://api.github.com/user");

                response.EnsureSuccessStatusCode();

                var json =
                    await response.Content
                        .ReadAsStringAsync();

                return JsonConvert.DeserializeObject<
                    GitHubProfileDto>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetPrimaryEmail(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();

                _httpClient.DefaultRequestHeaders.Add(
                    "Authorization",
                    $"Bearer {accessToken}");

                _httpClient.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "AutomationAPI");

                var response =
                    await _httpClient.GetAsync(
                        "https://api.github.com/user/emails");

                response.EnsureSuccessStatusCode();

                var json =
                    await response.Content
                        .ReadAsStringAsync();

                var emails =
                    JsonConvert.DeserializeObject<List<GitHubEmailDto>>(json);

                return emails?
                    .FirstOrDefault(e => e.Primary)
                    ?.Email;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }
    }
}
