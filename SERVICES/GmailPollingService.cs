using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Secrets;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.SERVICES
{
    public class GmailPollingService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<GmailPollingService> _logger;
        private readonly GmailIntegrationService _gmailService;
        private readonly ITriggerEngineService _triggerEngine;
        private readonly ISecretProtector _secretProtector;
        public GmailPollingService(AppDbContext dbContext, ILogger<GmailPollingService> logger, GmailIntegrationService gmailService, ITriggerEngineService triggerEngine,
            ISecretProtector secretProtector)
        {
            _dbContext = dbContext;
            _logger = logger;
            _gmailService = gmailService;
            _triggerEngine = triggerEngine;
            _secretProtector = secretProtector;
        }

        public async Task PollAsync()
        {
            try
            {
                var gmailConnections = await _dbContext.ConnectedApps
                        .Where(c => c.Provider == "google")
                        .ToListAsync();

                foreach (var connection in gmailConnections)
                {
                    try
                    {
                        await ProcessConnection(connection);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }

        private async Task ProcessConnection(ConnectedApp connection)
        {
            try
            {
                var emails = await _gmailService.GetLatestEmailAsync(_secretProtector.Unprotect(connection.AccessToken));

                foreach (var email in emails)
                {
                    if (email.Id == connection.LastSyncCursor)
                    {
                        break;
                    }

                    await _triggerEngine.ExecuteTriggerAsync(
                        new TriggerEvent
                        {
                            TriggerName = "gmail.new_email",
                            UserId = connection.UserId,
                            Payload = new Dictionary<string, object>
                            {
                            { "from", email.From  },
                            { "subject", email.Subject },
                            { "body", email.Body },
                            }
                        });
                }

                var latest = emails.FirstOrDefault();

                if (latest != null)
                {
                    connection.LastSyncCursor = latest.Id;
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
        }
    }
}
