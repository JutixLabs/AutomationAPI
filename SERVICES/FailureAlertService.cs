using AutomationAPI.DATA;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;

namespace AutomationAPI.SERVICES
{
    public class FailureAlertService : IFailureAlertService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<FailureAlertService> _logger;
        private readonly GmailIntegrationService _gmailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISecretProtector _secretProtector;
        public FailureAlertService(AppDbContext dbContext, ILogger<FailureAlertService> logger, GmailIntegrationService gmailService, IHttpContextAccessor httpContextAccessor,
            ISecretProtector secretProtector)
        {
            _dbContext = dbContext;
            _logger = logger;
            _gmailService = gmailService;
            _httpContextAccessor = httpContextAccessor;
            _secretProtector = secretProtector;
        }
        public async Task SendFailureAlertAsync(string userId, string message)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.ID == userId);
                var google = _dbContext.ConnectedApps
                    .FirstOrDefault(
                        g => g.UserId == userId && g.Provider == "google");

                if (google == null) 
                    throw new Exception("Google account not connected for user.");

                await _gmailService.SendEmail(
                    _secretProtector.Unprotect(google.AccessToken), 
                    user.Email, 
                    "Workflow Failure Alert", 
                    message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Failed to send failure alert: {ex.Message}");
            }
        }
    }
}
