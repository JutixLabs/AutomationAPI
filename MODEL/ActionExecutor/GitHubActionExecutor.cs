using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Newtonsoft.Json;

namespace AutomationAPI.MODEL.ActionExecutor
{
    public class GitHubActionExecutor : IActionExecutor
    {
        private readonly IGitHubProvider _gitHubProvider;
        private readonly IVariableResolver _variableResolver;
        private readonly ILogger<GitHubActionExecutor> _logger;

        public GitHubActionExecutor(IGitHubProvider gitHubProvider, IVariableResolver variableResolver,
            ILogger<GitHubActionExecutor> logger)
        {
            _gitHubProvider = gitHubProvider;
            _variableResolver = variableResolver;
            _logger = logger;
        }

        public string Provider => "github";

        public async Task ExecuteAsync(string userId, WorkFlowStep step, Dictionary<string, object> payload)
        {
            try
            {
                var config = ParseConfiguration(step);

                // "repo" is a resource-type field, so it's stored on ResourceId; ConfigurationJson
                // is kept as a fallback for older steps saved before that convention was set.
                var repo = step.ResourceId ?? GetField(config, "repo");

                if (string.IsNullOrWhiteSpace(repo))
                    throw new InvalidOperationException("GitHub action is missing a repository.");

                repo = _variableResolver.Resolve(repo, payload);

                switch (step.Action?.ToLower())
                {
                    case "github.create_issue":
                        {
                            var title = GetField(config, "title") ?? "Workflow Triggered Issue";
                            var description = GetField(config, "description") ?? string.Empty;

                            var resolvedTitle = _variableResolver.Resolve(title, payload);
                            var resolvedDescription = _variableResolver.Resolve(description, payload);

                            await _gitHubProvider.CreateIssueAsync(repo, resolvedTitle, resolvedDescription);
                            break;
                        }

                    case "github.create_branch":
                        {
                            var branchName = GetField(config, "branchName");

                            if (string.IsNullOrWhiteSpace(branchName))
                                throw new InvalidOperationException("GitHub create_branch is missing a branchName.");

                            var resolvedBranch = _variableResolver.Resolve(branchName, payload);

                            await _gitHubProvider.CreateBranchAsync(repo, resolvedBranch);
                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Unknown GitHub action: {step.Action}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] GitHub action {Action} failed: {Message}", step.Action, ex.Message);
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