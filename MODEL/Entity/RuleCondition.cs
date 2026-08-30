namespace AutomationAPI.MODEL.Entity
{
    public class RuleCondition
    {
        public int Id { get; set; }
        public int RuleStepId { get; set; }
        public WorkFlowStep RuleStep { get; set; }
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }
}
