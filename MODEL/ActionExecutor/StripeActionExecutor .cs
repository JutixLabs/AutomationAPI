using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Newtonsoft.Json;

namespace AutomationAPI.MODEL.ActionExecutor
{
    public class StripeActionExecutor : IActionExecutor
    {
        private readonly IStripeProvider _stripeProvider;
        private readonly IVariableResolver _variableResolver;
        private readonly ILogger<StripeActionExecutor> _logger;

        public StripeActionExecutor(IStripeProvider stripeProvider, IVariableResolver variableResolver,
            ILogger<StripeActionExecutor> logger)
        {
            _stripeProvider = stripeProvider;
            _variableResolver = variableResolver;
            _logger = logger;
        }

        public string Provider => "stripe";

        public async Task ExecuteAsync(string userId, WorkFlowStep step, Dictionary<string, object> payload)
        {
            try
            {
                var config = ParseConfiguration(step);

                switch (step.Action?.ToLower())
                {
                    case "stripe.create_customer":
                        {
                            var email = GetField(config, "email");
                            var name = GetField(config, "name") ?? string.Empty;

                            if (string.IsNullOrWhiteSpace(email))
                                throw new InvalidOperationException("Stripe create_customer is missing an email.");

                            var resolvedEmail = _variableResolver.Resolve(email, payload);
                            var resolvedName = _variableResolver.Resolve(name, payload);

                            await _stripeProvider.CreateCustomerAsync(userId, resolvedEmail, resolvedName);
                            break;
                        }

                    case "stripe.create_refund":
                        {
                            var chargeId = GetField(config, "chargeId");
                            if (string.IsNullOrEmpty(chargeId) && payload.ContainsKey("chargeId"))
                                chargeId = payload["chargeId"]?.ToString();

                            if (string.IsNullOrWhiteSpace(chargeId))
                                throw new InvalidOperationException("Stripe create_refund is missing a chargeId.");

                            var resolvedChargeId = _variableResolver.Resolve(chargeId, payload);

                            await _stripeProvider.CreateRefundAsync(userId, resolvedChargeId);
                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Unknown Stripe action: {step.Action}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Stripe action {Action} failed: {Message}", step.Action, ex.Message);
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