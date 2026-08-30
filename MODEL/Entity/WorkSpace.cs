namespace AutomationAPI.MODEL.Entity
{
    public class WorkSpace
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public List<Folder> Folders { get; set; } = new();
    }
}
