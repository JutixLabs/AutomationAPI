using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;
using AutomationAPI.SERVICES.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AutomationAPI.SERVICES.Providers
{
    public class SlackProvider : ISlackProvider
    {
        private readonly HttpClient _httpClient;
        private readonly SlackService _slackService;
        private readonly IConnectAppsService _connectAppsService;
        public SlackProvider(SlackService slackService, HttpClient httpClient, IConnectAppsService connectAppsService)
        {
            _slackService = slackService;
            _connectAppsService = connectAppsService;
            _httpClient = httpClient;
        }

        public async Task<List<SlackChannelDto>> GetChannelsAsync()
        {
            var connection = await _connectAppsService.GetConnectionAsync("slack");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await _connectAppsService.GetAccessTokenAsync("slack"));

            var response = await _httpClient.GetAsync("https://slack.com/api/conversations.list");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<
                SlackChannelsResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null || !result.Ok || result.Channels == null)
            {
                return new List<SlackChannelDto>();
            }

            return result.Channels
                .Select(x => new SlackChannelDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();
        }
    }
}
