namespace AutomationAPI.MODEL.DTO
{
    public class TriggerDefinition
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Provider { get; set; }
        public string Description { get; set; }
        public bool RequiresConnection { get; set; }
        public List<TriggerField> PayloadFields { get; set; }
    }
}
