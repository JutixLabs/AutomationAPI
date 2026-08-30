using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.SERVICES
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AnalyticsService> _logger;
        public AnalyticsService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<AnalyticsService> logger)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<AnalyticsDTO> GetSummaryAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            try
            {
                var totalRules = _dbContext.AutomationRules
                        .Count(r => r.UserID == userId);

                var totalRuns = _dbContext.ExecutionLogs
                    .Count(r => r.UserId == userId);

                var successRuns = _dbContext.ExecutionLogs
                    .Count(r => r.UserId == userId && r.Status == "Success");

                var failRuns = totalRuns - successRuns;

                var analytics = new AnalyticsDTO
                {
                    TotalRules = totalRules,
                    TotalRuns = totalRuns,
                    SuccessRuns = successRuns,
                    FailedRuns = failRuns
                };

                return analytics;
            }
            catch (Exception ex) 
            {
                _logger.LogError($"[ERROR] {ex.Message}");
                throw;
            }
        }

    }
}
