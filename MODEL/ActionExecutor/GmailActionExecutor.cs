using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;
using AutomationAPI.SERVICES.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.MODEL.ActionExecutor
{
    public class GmailActionExecutor : IActionExecutor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;
        private readonly GmailIntegrationService _gmailService;
        private readonly IVariableResolver _variableResolver;
        private readonly ILogger<GmailActionExecutor> _logger;
        private readonly ISecretProtector _secretProtector;
        public GmailActionExecutor(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext, GmailIntegrationService gmailService, 
            IVariableResolver variableResolver, ILogger<GmailActionExecutor> logger, ISecretProtector secretProtector)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _gmailService = gmailService;
            _variableResolver = variableResolver;
            _logger = logger;
            _secretProtector = secretProtector;
        }
        public string Provider => "gmail";

        public async Task ExecuteAsync(string userId, WorkFlowStep step, Dictionary<string, object> payload)
        {
            try
            {
                var google = _dbContext.ConnectedApps
                    .FirstOrDefault(g => g.UserId == userId && g.Provider == "google");

                if (google == null)
                    throw new InvalidOperationException("Google not connected.");

                var body = payload.ContainsKey("message")
                    ? payload["message"].ToString()
                    : "Workflow Triggered";

                var resolvedTarget = _variableResolver.Resolve(step.Target, payload);
                var resolvedBody = _variableResolver.Resolve(body, payload);

                await _gmailService.SendEmail(
                    _secretProtector.Unprotect(google.AccessToken),
                    resolvedTarget,
                    "Automation Trigger",
                    resolvedBody);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }
    }
}
