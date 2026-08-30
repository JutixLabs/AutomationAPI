using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace AutomationAPI.MODEL.ActionExecutor
{
    public class NotionActionExecutor : IActionExecutor
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly IVariableResolver _variableResolver;
        private readonly ILogger<NotionActionExecutor> _logger;
        private readonly ISecretProtector _secretProtector;
        public NotionActionExecutor(HttpClient httpClient, AppDbContext dbContext,
            IVariableResolver variableResolver, ILogger<NotionActionExecutor> logger, ISecretProtector secretProtector)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _variableResolver = variableResolver;
            _logger = logger;
            _secretProtector = secretProtector;
        }

        public string Provider => "notion";

        public async Task ExecuteAsync(string userId, WorkFlowStep step, Dictionary<string, object> payload)
        {
            try
            {
                var notion = await _dbContext.ConnectedApps
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == "notion" && x.IsActive);

                if (notion == null)
                    throw new InvalidOperationException("Notion not connected.");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _secretProtector.Unprotect(notion.AccessToken));
                _httpClient.DefaultRequestHeaders.Remove("Notion-Version");
                _httpClient.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");

                var config = ParseConfiguration(step);
                var databaseId = step.ResourceId ?? GetField(config, "databaseId");

                if (string.IsNullOrWhiteSpace(databaseId))
                    throw new InvalidOperationException("Notion action is missing a databaseId.");

                switch (step.Action?.ToLower())
                {
                    case "notion.create_page":
                        {
                            var title = GetField(config, "title") ?? "Untitled";
                            var resolvedTitle = _variableResolver.Resolve(title, payload);

                            var body = new
                            {
                                parent = new { database_id = databaseId },
                                properties = new Dictionary<string, object>
                                {
                                    ["Name"] = new
                                    {
                                        title = new object[]
                                        {
                                            new { text = new { content = resolvedTitle } }
                                        }
                                    }
                                }
                            };

                            var response = await _httpClient.PostAsJsonAsync("https://api.notion.com/v1/pages", body);

                            if (!response.IsSuccessStatusCode)
                            {
                                var respBody = await response.Content.ReadAsStringAsync();
                                throw new HttpRequestException($"Notion create-page failed ({response.StatusCode}): {respBody}");
                            }

                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Unknown Notion action: {step.Action}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Notion action {Action} failed: {Message}", step.Action, ex.Message);
                throw;
            }
        }

        private static Dictionary<string, string> ParseConfiguration(WorkFlowStep step)
        {
            if (string.IsNullOrWhiteSpace(step.ConfigurationJson))
                return new Dictionary<string, string>();

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(step.ConfigurationJson)
                       ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, string>();
            }
        }

        private static string GetField(Dictionary<string, string> config, string key)
        {
            return config.TryGetValue(key, out var value) ? value : null;
        }
    }
}