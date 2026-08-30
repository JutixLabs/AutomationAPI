using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Secrets;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutomationAPI.SERVICES.Providers
{
    public class TrelloProvider : ITrelloProvider
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly ISecretProtector _secretProtector;
        public TrelloProvider(HttpClient httpClient, AppDbContext dbContext, IConfiguration config,
            ISecretProtector secretProtector)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _config = config;
            _secretProtector = secretProtector;
        }

        // Trello auth is passed as query params on every request, not a header — this
        // builds that shared "key=...&token=..." suffix for the connected user.
        private async Task<string> AuthQueryAsync(string userId)
        {
            var trello = await _dbContext.ConnectedApps
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == "trello" && x.IsActive);

            if (trello == null)
                throw new InvalidOperationException("Trello not connected.");

            var apiKey = _config["TrelloOAuth:ApiKey"];

            return $"key={apiKey}&token={_secretProtector.Unprotect(trello.AccessToken)}";
        }

        public async Task<List<TrelloBoardDto>> GetBoardsAsync(string userId)
        {
            var auth = await AuthQueryAsync(userId);

            var response = await _httpClient.GetAsync($"https://api.trello.com/1/members/me/boards?{auth}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TrelloBoardDto>>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<List<TrelloListDto>> GetListsAsync(string userId, string boardId)
        {
            var auth = await AuthQueryAsync(userId);

            var response = await _httpClient.GetAsync($"https://api.trello.com/1/boards/{boardId}/lists?{auth}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TrelloListDto>>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task CreateCardAsync(string userId, string listId, string name, string description)
        {
            var auth = await AuthQueryAsync(userId);

            var response = await _httpClient.PostAsync(
                $"https://api.trello.com/1/cards?{auth}" +
                $"&idList={listId}&name={Uri.EscapeDataString(name)}&desc={Uri.EscapeDataString(description ?? "")}",
                null);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Trello create-card failed ({response.StatusCode}): {body}");
            }
        }

        public async Task AddCommentAsync(string userId, string cardId, string text)
        {
            var auth = await AuthQueryAsync(userId);

            var response = await _httpClient.PostAsync(
                $"https://api.trello.com/1/cards/{cardId}/actions/comments?{auth}&text={Uri.EscapeDataString(text)}",
                null);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Trello add-comment failed ({response.StatusCode}): {body}");
            }
        }

        public async Task<string> CreateWebhookAsync(string userId, string boardId, string callbackUrl)
        {
            var auth = await AuthQueryAsync(userId);

            // Trello sends a HEAD request to callbackUrl to verify it's reachable before
            // accepting this — the receiving endpoint must already be deployed and public.
            var response = await _httpClient.PostAsync(
                $"https://api.trello.com/1/webhooks/?{auth}" +
                $"&callbackURL={Uri.EscapeDataString(callbackUrl)}" +
                $"&idModel={boardId}" +
                $"&description={Uri.EscapeDataString("AutomationsAPI trigger")}",
                null);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Trello webhook creation failed ({response.StatusCode}): {json}");

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("id").GetString();
        }

        public async Task DeleteWebhookAsync(string userId, string webhookId)
        {
            var auth = await AuthQueryAsync(userId);

            var response = await _httpClient.DeleteAsync($"https://api.trello.com/1/webhooks/{webhookId}?{auth}");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Trello webhook deletion failed ({response.StatusCode}): {body}");
            }
        }
    }
}