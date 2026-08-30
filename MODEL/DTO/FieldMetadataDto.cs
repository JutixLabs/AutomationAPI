namespace AutomationAPI.MODEL.DTO
{
    public class FieldMetadataDto
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public string ResourceType { get; set; }
        public string? DependsOn { get; set; }
        public bool Required { get; set; } = true;
        public string Placeholder { get; set; }
    }
}
