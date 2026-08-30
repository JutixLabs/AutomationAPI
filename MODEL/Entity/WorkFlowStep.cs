namespace AutomationAPI.MODEL.Entity
{
    public class WorkFlowStep
    {
        public int Id { get; set; }
        public int AutomationRuleId { get; set; }
        public AutomationRule AutomationRule { get; set; }
        public int Order { get; set; }
        public string? Action { get; set; }
        public string? Target { get; set; }
        public string? ResourceId { get; set; }
        public bool IsBranchStep { get; set; }
        public int? TrueStepId { get; set; }
        public int? FalseStepId { get; set; }
        public int? DelayAmount { get; set; }
        public string? DelayUnit { get; set; }
        public int RetryCount { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 30;
        public bool IsLoopStep { get; set; }
        public string? LoopField { get; set; }
        public string? ConfigurationJson { get; set; }
        public ICollection<RuleCondition> Conditions { get; set; }
    }
}
