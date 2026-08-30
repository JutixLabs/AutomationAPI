using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IAnalyticsService
    {
        Task<AnalyticsDTO> GetSummaryAsync();
        //Task GetRulesPerRuleStatsAsync();
        //Task GetWeeklyStatsAsync();
    }
}
