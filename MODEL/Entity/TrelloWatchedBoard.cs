namespace AutomationAPI.MODEL.Entity
{
    public class TrelloWatchedBoard
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string BoardId { get; set; }
        public string BoardName { get; set; }
        public string TrelloWebhookId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
