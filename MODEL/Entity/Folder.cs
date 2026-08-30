namespace AutomationAPI.MODEL.Entity
{
    public class Folder
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int WorkSpaceId { get; set; }
        public WorkSpace WorkSpace { get; set; }
        public List<AutomationRule> Rules { get; set; } = new();
    }
}
