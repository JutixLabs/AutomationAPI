namespace AutomationAPI.MODEL.DTO
{
    public class GoogleFormResponseDto
    {
        public string ResponseId { get; set; }
        public string FormId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public Dictionary<string, string> Answers { get; set; }
    }
}
