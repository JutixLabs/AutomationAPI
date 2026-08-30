using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;

namespace AutomationAPI.SERVICES
{
    public class ConditionEvaluator : IConditionEvaluator
    {
        public bool Evaluate(RuleCondition condition, Dictionary<string, object> payload)
        {
            if (!payload.ContainsKey(condition.Field))
                return false;

            var payloadValue = payload[condition.Field]?.ToString();

            var compareValue = condition.Value;

            switch (condition.Operator.ToLower())
            {
                case "equals":
                    return payloadValue == compareValue;

                case "contains":
                    return payloadValue.Contains(compareValue);

                case "starts_with":
                    return payloadValue.StartsWith(compareValue);

                case "ends_with":
                    return payloadValue.EndsWith(compareValue);

                default: 
                    return false;
            }
        }
    }
}
