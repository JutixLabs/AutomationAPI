using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Newtonsoft.Json;

namespace AutomationAPI.MODEL.ActionExecutor
{
    public class DiscordActionExecutor : IActionExecutor
    {
        private readonly IDiscordProvider _discordProvider;
        private readonly IVariableResolver _variableResolver;
        private readonly ILogger<DiscordActionExecutor> _logger;

        public DiscordActionExecutor(IDiscordProvider discordProvider, IVariableResolver variableResolver,
            ILogger<DiscordActionExecutor> logger)
        {
            _discordProvider = discordProvider;
            _variableResolver = variableResolver;
            _logger = logger;
        }

        public string Provider => "discord";

        public async Task ExecuteAsync(string userId, WorkFlowStep step, Dictionary<string, object> payload)
        {
            try
            {
                var config = ParseConfiguration(step);

                // channelId is always required; Target is kept as a fallback for steps saved
                // before ConfigurationJson support existed.
                var channelId = GetField(config, "channelId") ?? step.Target;

                if (string.IsNullOrWhiteSpace(channelId))
                    throw new InvalidOperationException("Discord action is missing a channelId.");

                channelId = _variableResolver.Resolve(channelId, payload);

                switch (step.Action?.ToLower())
                {
                    case "discord.send_message":
                        {
                            var message = GetField(config, "message");
                            if (string.IsNullOrEmpty(message) && payload.ContainsKey("message"))
                                message = payload["message"]?.ToString();
                            message ??= "Workflow Triggered.";

                            var resolvedMessage = _variableResolver.Resolve(message, payload);

                            await _discordProvider.SendMessageAsync(channelId, resolvedMessage);
                            break;
                        }

                    case "discord.create_thread":
                        {
                            var threadName = GetField(config, "threadName") ?? "Workflow Thread";
                            var resolvedName = _variableResolver.Resolve(threadName, payload);

                            await _discordProvider.CreateThreadAsync(channelId, resolvedName);
                            break;
                        }

                    case "discord.delete_message":
                        {
                            var messageId = GetField(config, "messageId");

                            if (string.IsNullOrWhiteSpace(messageId))
                                throw new InvalidOperationException("Discord delete_message is missing a messageId.");

                            var resolvedMessageId = _variableResolver.Resolve(messageId, payload);

                            await _discordProvider.DeleteMessageAsync(channelId, resolvedMessageId);
                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Unknown Discord action: {step.Action}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Discord action {Action} failed: {Message}", step.Action, ex.Message);
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