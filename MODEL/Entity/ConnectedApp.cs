namespace AutomationAPI.MODEL.Entity
{
    public class ConnectedApp
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string Provider { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string ExternalAccountId { get; set; } 
        public string ExternalAccountEmail { get; set; } 
        public string MetaDataJson { get; set; }
        public string LastSyncCursor { get; set; }
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow; 
    }
}
