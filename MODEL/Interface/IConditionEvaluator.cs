using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IConditionEvaluator
    {
        bool Evaluate(RuleCondition condition, Dictionary<string, object> payload);
    }
}
