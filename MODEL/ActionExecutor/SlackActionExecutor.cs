using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;
using AutomationAPI.SERVICES.Persistence;

namespace AutomationAPI.MODEL.ActionExecutor
{
    public class SlackActionExecutor : IActionExecutor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;
        private readonly SlackService _slackService;
        private readonly IVariableResolver _variableResolver;
        private readonly ILogger<SlackActionExecutor> _logger;
        private readonly ISecretProtector _secretProtector;
        public SlackActionExecutor(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext, SlackService slackService, 
            IVariableResolver variableResolver, ILogger<SlackActionExecutor> logger, ISecretProtector secretProtector)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _slackService = slackService;
            _variableResolver = variableResolver;
            _logger = logger;
            _secretProtector = secretProtector;
        }
        public string Provider => "slack";

        public async Task ExecuteAsync(string userId, WorkFlowStep step, Dictionary<string, object> payload)
        {
            try
            {
                var slack = _dbContext.ConnectedApps
                    .FirstOrDefault(s => s.UserId == userId && s.Provider == "slack");

                if (slack == null)
                    throw new InvalidOperationException("Slack not connected.");

                var body = payload.ContainsKey("message")
                    ? payload["message"].ToString()
                    : "Workflow Triggered.";

                var resolvedTarget = _variableResolver.Resolve(step.Target, payload);
                var resolvedBody = _variableResolver.Resolve(body, payload);

                await _slackService.SendMessage(
                    _secretProtector.Unprotect(slack.AccessToken),
                    resolvedTarget,
                    resolvedBody);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }
    }
}
