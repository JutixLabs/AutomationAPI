namespace AutomationAPI.MODEL.DTO
{
    public class ActionMetadataDto
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public List<FieldMetadataDto> Fields { get; set; } = new();
    }
}
