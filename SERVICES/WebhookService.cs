using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutomationAPI.SERVICES
{
    public class WebhookService : IWebhookService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<WebhookService> _logger;
        private readonly GmailIntegrationService _gmailService;
        private readonly ISecretProtector _secretProtector;
        public WebhookService(AppDbContext dbContext, ILogger<WebhookService> logger, GmailIntegrationService gmailService, ISecretProtector secretProtector)
        {
            _dbContext = dbContext;
            _logger = logger;
            _gmailService = gmailService;
            _secretProtector = secretProtector;
        }
        public async Task ReceiveAsync(string key, JsonElement payload)
        {
            try
            {
                var rule = await _dbContext.AutomationRules.FirstOrDefaultAsync(r => r.WebhookKey == key);
                if (rule == null)
                    throw new Exception("Invalid webhook key.");

                await ExecuteRuleAsync(rule.ID, payload);

                _logger.LogInformation($"[INFO] Webhook received for rule ID: {rule.ID}");
                Console.WriteLine(payload.ValueKind);
                Console.WriteLine(payload.GetRawText());
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

        public async Task ExecuteRuleAsync(int ruleId, JsonElement payload)
        {
            try
            {
                var rule = await _dbContext.AutomationRules
                        .Where(r => r.ID == ruleId)
                        .Select(r => new
                        {
                            r.ID,
                            r.Trigger,
                            r.UserID,
                            Steps = r.Steps.ToList()
                        })
                        .FirstOrDefaultAsync();
                _logger.LogInformation($"[INFO] Executing rule ID: {ruleId} with payload: {JsonSerializer.Serialize(payload)}");
                var webhookExec = await _dbContext.WorkflowExecutions.AddAsync(
                    new WorkflowExecution
                    {
                        RuleId = ruleId,
                        Payload = payload.GetRawText(),
                        Success = true,
                        Error = "",
                    });
                await _dbContext.SaveChangesAsync();

                if (rule == null)
                    throw new Exception("Rule not found.");

                var gmail = await _dbContext.ConnectedApps
                    .FirstOrDefaultAsync(g => g.UserId == rule.UserID && g.Provider == "google");

                foreach (var step in rule.Steps)
                {
                    switch (step.Action)
                    {
                        case "send_email":

                            if (gmail != null)
                            {
                                await _gmailService.SendEmail(
                                    _secretProtector.Unprotect(gmail.AccessToken),
                                    step.Target,
                                    "Automation Triggered",
                                    $"Webhook received:\n\n{payload}"
                                );
                            }

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                await _dbContext.WorkflowExecutions.AddAsync(
                    new WorkflowExecution
                    {
                        RuleId = ruleId,
                        Payload = payload.GetRawText(),
                        Success = false,
                        Error = ex.Message,
                    });
                throw;
            }
        }
    }
}
