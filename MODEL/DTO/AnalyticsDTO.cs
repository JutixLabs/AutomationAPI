namespace AutomationAPI.MODEL.DTO
{
    public class AnalyticsDTO
    {
        public int TotalRules { get; set; }
        public int TotalRuns { get; set; }
        public int SuccessRuns { get; set; }
        public int FailedRuns { get; set; }
    }

    public class StatsDTO
    {
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
