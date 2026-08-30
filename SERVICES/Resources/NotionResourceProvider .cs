using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;
using AutomationAPI.SERVICES.Secrets;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AutomationAPI.SERVICES.Resources
{
    public class NotionResourceProvider : IResourceProvider
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISecretProtector _secretProtector;
        public NotionResourceProvider(HttpClient httpClient, AppDbContext dbContext, IHttpContextAccessor httpContextAccessor,
            ISecretProtector secretProtector)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _secretProtector = secretProtector;
        }

        public string ProviderName() => "notion";

        public async Task<List<ResourceOptionDto>> GetResourcesAsync(string resourceType, Dictionary<string, string> filters = null)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            var notion = await _dbContext.ConnectedApps
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == "notion" && x.IsActive);

            if (notion == null)
                throw new InvalidOperationException("Notion not connected.");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _secretProtector.Unprotect(notion.AccessToken));
            _httpClient.DefaultRequestHeaders.Remove("Notion-Version");
            _httpClient.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");

            switch (resourceType)
            {
                case "database":
                    {
                        var response = await _httpClient.PostAsJsonAsync(
                            "https://api.notion.com/v1/search",
                            new { filter = new { property = "object", value = "database" } });

                        response.EnsureSuccessStatusCode();
                        var json = await response.Content.ReadAsStringAsync();

                        using var doc = JsonDocument.Parse(json);
                        var results = new List<ResourceOptionDto>();

                        foreach (var item in doc.RootElement.GetProperty("results").EnumerateArray())
                        {
                            var id = item.GetProperty("id").GetString();
                            var titleArray = item.GetProperty("title").EnumerateArray();
                            var name = titleArray.Any()
                                ? titleArray.First().GetProperty("plain_text").GetString()
                                : "(Untitled database)";

                            results.Add(new ResourceOptionDto { Id = id, Name = name });
                        }

                        return results;
                    }

                default:
                    return new List<ResourceOptionDto>();
            }
        }
    }
}