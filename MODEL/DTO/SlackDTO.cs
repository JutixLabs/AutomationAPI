namespace AutomationAPI.MODEL.DTO
{
    public class SlackProfileDto
    {
        public bool Ok { get; set; }

        public SlackUser User { get; set; }
    }

    public class SlackUser
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}
