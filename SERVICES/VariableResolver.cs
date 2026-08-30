using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace AutomationAPI.SERVICES
{
    public class VariableResolver : IVariableResolver
    {
        private readonly ILogger<VariableResolver> _logger;
        public VariableResolver(ILogger<VariableResolver> logger)
        {
            _logger = logger;
        }

        public string Resolve(string template, Dictionary<string, object> payload)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(template))
                    return template;

                if (payload == null || payload.Count == 0)
                    return template;

                var matches = Regex.Matches(template, @"\{\{(.*?)\}\}");
                if (matches.Count == 0)
                    return template;

                // Wrapping the payload as a JObject lets one lookup handle both flat
                // dictionaries (Gmail/Slack build these by hand — {{from}}, {{channel}})
                // and deeply nested raw webhook bodies (GitHub/Stripe pass their
                // provider's JSON straight through — {{issue.title}},
                // {{data.object.amount}}) the same way, via dot-path traversal.
                var root = JObject.FromObject(payload);

                foreach (Match match in matches)
                {
                    var key = match.Groups[1].Value.Trim();

                    JToken token;
                    try
                    {
                        token = root.SelectToken(key);
                    }
                    catch (Exception)
                    {
                        // Malformed path syntax in one variable shouldn't blow up the
                        // whole template — just leave that one placeholder unresolved.
                        continue;
                    }

                    if (token == null || token.Type == JTokenType.Null)
                        continue;

                    var value = (token.Type == JTokenType.Object || token.Type == JTokenType.Array)
                        ? token.ToString(Newtonsoft.Json.Formatting.None)
                        : token.ToString();

                    template = template.Replace(match.Value, value);
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public string Resolve(string input, WorkflowExecutionContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                    return input;

                foreach (var variable in context.Variables)
                {
                    input = input.Replace(
                        $"{{{{{variable.Key}}}}}",
                        variable.Value?.ToString());
                }

                return input;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }
    }
}
