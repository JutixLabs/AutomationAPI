namespace AutomationAPI.MODEL.DTO
{
    public class GitHubRepositoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Full_Name { get; set; }
        public string Default_Branch { get; set; }
        public bool Private { get; set; }
    }
}
