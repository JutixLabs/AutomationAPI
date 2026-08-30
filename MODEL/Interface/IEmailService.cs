namespace AutomationAPI.MODEL.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        public string LoadEmailTemplate(string filePath, Dictionary<string, string> replacements);
    }
}
