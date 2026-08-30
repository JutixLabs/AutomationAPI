namespace AutomationAPI.MODEL.Entity
{
    public class DiscordWatchedChannel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ServerId { get; set; }
        public string ServerName { get; set; }
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string LastMessageId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
