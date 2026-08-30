using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Newtonsoft.Json;

namespace AutomationAPI.MODEL.ActionExecutor
{
    public class TrelloActionExecutor : IActionExecutor
    {
        private readonly ITrelloProvider _trelloProvider;
        private readonly IVariableResolver _variableResolver;
        private readonly ILogger<TrelloActionExecutor> _logger;

        public TrelloActionExecutor(ITrelloProvider trelloProvider, IVariableResolver variableResolver,
            ILogger<TrelloActionExecutor> logger)
        {
            _trelloProvider = trelloProvider;
            _variableResolver = variableResolver;
            _logger = logger;
        }

        public string Provider => "trello";

        public async Task ExecuteAsync(string userId, WorkFlowStep step, Dictionary<string, object> payload)
        {
            try
            {
                var config = ParseConfiguration(step);

                switch (step.Action?.ToLower())
                {
                    case "trello.create_card":
                        {
                            var listId = step.ResourceId ?? GetField(config, "listId");
                            var name = GetField(config, "name") ?? "Workflow Triggered Card";
                            var description = GetField(config, "description") ?? string.Empty;

                            if (string.IsNullOrWhiteSpace(listId))
                                throw new InvalidOperationException("Trello create_card is missing a listId.");

                            var resolvedName = _variableResolver.Resolve(name, payload);
                            var resolvedDescription = _variableResolver.Resolve(description, payload);

                            await _trelloProvider.CreateCardAsync(userId, listId, resolvedName, resolvedDescription);
                            break;
                        }

                    case "trello.add_comment":
                        {
                            var cardId = step.ResourceId ?? GetField(config, "cardId");
                            var text = GetField(config, "text") ?? string.Empty;

                            if (string.IsNullOrWhiteSpace(cardId))
                                throw new InvalidOperationException("Trello add_comment is missing a cardId.");

                            var resolvedText = _variableResolver.Resolve(text, payload);

                            await _trelloProvider.AddCommentAsync(userId, cardId, resolvedText);
                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Unknown Trello action: {step.Action}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Trello action {Action} failed: {Message}", step.Action, ex.Message);
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