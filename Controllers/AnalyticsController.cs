using AutomationAPI.DATA;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;
using AutomationAPI.SERVICES.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AnalyticsController(IAnalyticsService analyticsService, AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _analyticsService = analyticsService;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize]
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _analyticsService.GetSummaryAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("perRuleStats")]
        public IActionResult GetRulesPerRuleStats()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            var data = _dbContext.ExecutionLogs
                .Where(e => e.UserId == userId)
                .GroupBy(e => new { e.Trigger, e.Action })
                .Select(g => new
                {
                    trigger = g.Key.Trigger,
                    action = g.Key.Action,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .Take(5)
                .ToList();

            return Ok(data);
        }

        [Authorize]
        [HttpGet("weeklyStats")]
        public IActionResult GetWeeklyActivity()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();


            var startDate = DateTime.UtcNow.Date.AddDays(-6);

            var logs = _dbContext.ExecutionLogs
                .Where(e => e.UserId == userId && e.ExecutedAt >= startDate)
                .ToList();

            var result = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var day = startDate.AddDays(i);
                    var count = logs.Count(l => l.ExecutedAt.Date == day);

                    return new
                    {
                        date = day.ToString("yyyy-MM-dd"),
                        count
                    };
                });

            return Ok(result);
        }
    }
}
