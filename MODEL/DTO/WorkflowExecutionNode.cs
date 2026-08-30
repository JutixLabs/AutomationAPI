using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.DTO
{
    public class WorkflowExecutionNode
    {
        public WorkFlowStep Step { get; set; }
        public bool ConditionPassed { get; set; }
    }
}
