using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace AutomationAPI.SERVICES
{
    public class DiscordPollingService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DiscordPollingService> _logger;
        private readonly ITriggerEngineService _triggerEngine;
        private readonly IIntegrationCredentialService _credentials;
        private readonly IHttpClientFactory _httpClientFactory;

        public DiscordPollingService(AppDbContext dbContext, ILogger<DiscordPollingService> logger,
            ITriggerEngineService triggerEngine, IIntegrationCredentialService credentials,
            IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _logger = logger;
            _triggerEngine = triggerEngine;
            _credentials = credentials;
            _httpClientFactory = httpClientFactory;
        }

        public async Task PollAsync()
        {
            try
            {
                var botToken = await _credentials.GetCredentialAsync("discord");
                if (string.IsNullOrEmpty(botToken))
                {
                    _logger.LogWarning("[WARN] Discord bot token not configured — skipping poll.");
                    return;
                }

                var watchedChannels = await _dbContext.DiscordWatchedChannels.ToListAsync();
                if (watchedChannels.Count == 0)
                    return;

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);

                foreach (var channel in watchedChannels)
                {
                    try
                    {
                        var url = $"https://discord.com/api/channels/{channel.ChannelId}/messages?limit=20";
                        if (!string.IsNullOrEmpty(channel.LastMessageId))
                            url += $"&after={channel.LastMessageId}";

                        var response = await client.GetAsync(url);

                        if (!response.IsSuccessStatusCode)
                        {
                            var errBody = await response.Content.ReadAsStringAsync();
                            _logger.LogError("[ERROR] Discord poll failed for channel {ChannelId} ({Status}): {Body}",
                                channel.ChannelId, response.StatusCode, errBody);
                            continue;
                        }

                        var json = await response.Content.ReadAsStringAsync();
                        var messages = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

                        if (messages == null || messages.Count == 0)
                            continue;

                        // Discord returns newest-first — reverse so triggers fire in send order.
                        messages.Reverse();

                        foreach (var message in messages)
                        {
                            await _triggerEngine.ExecuteTriggerAsync(new TriggerEvent
                            {
                                TriggerName = "discord.message_created",
                                UserId = channel.UserId,
                                Payload = message
                            });
                        }

                        // Message IDs are Discord snowflakes — numerically increasing over time —
                        // so the last item after reversing is the newest one fetched this pass.
                        channel.LastMessageId = messages.Last()["id"].ToString();
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[ERROR] Exception polling Discord channel {ChannelId}.", channel.ChannelId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Discord polling outer loop error.");
            }
        }
    }
}