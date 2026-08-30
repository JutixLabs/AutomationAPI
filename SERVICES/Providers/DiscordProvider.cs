using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AutomationAPI.SERVICES.Providers
{
    public class DiscordProvider : IDiscordProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConnectAppsService _connectedAppsService;
        private readonly IIntegrationCredentialService _credentials;
        public DiscordProvider(HttpClient httpClient, IConnectAppsService connectAppsService, IIntegrationCredentialService credentials)
        {
            _httpClient = httpClient;
            _connectedAppsService = connectAppsService;
            _credentials = credentials;
        }

        public async Task CreateThreadAsync(string channelId, string title)
        {
            await AuthenticateBot();

            var response = await _httpClient.PostAsJsonAsync(
                $"https://discord.com/api/channels/{channelId}/threads",
                new
                {
                    name = title,
                    // 11 = PUBLIC_THREAD (Discord "Channel Types"). Use 12 (PRIVATE_THREAD)
                    // instead if the parent channel requires it.
                    type = 11
                });

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Discord create-thread failed ({response.StatusCode}): {body}");
            }
        }

        public async Task DeleteMessageAsync(string channelId, string messageId)
        {
            await AuthenticateBot();

            var response = await _httpClient.DeleteAsync(
                $"https://discord.com/api/channels/{channelId}/messages/{messageId}");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Discord delete-message failed ({response.StatusCode}): {body}");
            }
        }

        public async Task<List<DiscordChannelDto>> GetChannelsAsync(string guildId)
        {
            await Authenticate();

            var response = await _httpClient.GetAsync($"https://discord.com/api/guilds/{guildId}/channels");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<DiscordChannelDto>>(json);
        }

        public async Task<List<DiscordGuildDto>> GetServersAsync()
        {
            await Authenticate();

            var response = await _httpClient.GetAsync("https://discord.com/api/users/@me/guilds");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<DiscordGuildDto>>(json);
        }

        public async Task SendMessageAsync(string channelId, string message)
        {
            await AuthenticateBot();

            var response = await _httpClient.PostAsJsonAsync(
                $"https://discord.com/api/channels/{channelId}/messages",
                new
                {
                    content = message
                });

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Discord send-message failed ({response.StatusCode}): {body}");
            }
        }

        // Listing a user's own servers/channels uses their personal OAuth token (scope: "guilds").
        private async Task Authenticate()
        {
            var token =
                await _connectedAppsService
                    .GetAccessTokenAsync("discord");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }

        // Posting/managing messages requires a Discord *bot* token — the bot must be invited into
        // the target server separately from the user's OAuth connection. Store this bot token via
        // IIntegrationCredentialService (provider: "discord") — it's app-wide, not per-user.
        private async Task AuthenticateBot()
        {
            var token = await _credentials.GetCredentialAsync("discord");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bot", token);
        }
    }
}