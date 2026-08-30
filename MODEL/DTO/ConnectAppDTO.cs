namespace AutomationAPI.MODEL.DTO
{
    public class ConnectAppDTO
    {
        public int Id { get; set; }
        public string Provider { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool IsActive { get; set; }
        public string ExternalAccountEmail { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
