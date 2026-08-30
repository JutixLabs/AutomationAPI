namespace AutomationAPI.MODEL.DTO
{
    public class GitHubProfileDto
    {
        public int Id { get; set; }

        public string Login { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Avatar_Url { get; set; }
    }

    public class GitHubEmailDto
    {
        public string Email { get; set; }

        public bool Primary { get; set; }

        public bool Verified { get; set; }
    }
}
