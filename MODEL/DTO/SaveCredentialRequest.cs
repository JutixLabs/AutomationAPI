namespace AutomationAPI.MODEL.DTO
{
    public class SaveCredentialRequest
    {
        public string Provider { get; set; }
        public string Name { get; set; }
        public string CredentialType { get; set; }
        public string Value { get; set; }
    }
}
