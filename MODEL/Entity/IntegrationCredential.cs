namespace AutomationAPI.MODEL.Entity
{
    public class IntegrationCredential
    {
        public int Id { get; set; }
        public string Provider { get; set; }
        public string Name { get; set; }
        public string CredentialType { get; set; }
        public string Value { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
