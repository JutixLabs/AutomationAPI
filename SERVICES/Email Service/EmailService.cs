using AutomationAPI.MODEL.Interface;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AutomationAPI.SERVICES.Email_Service
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;
        public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        public string LoadEmailTemplate(string filePath, Dictionary<string, string> replacements)
        {
            try
            {
                var body = File.ReadAllText(filePath);
                foreach (var item in replacements)
                {
                    body = body.Replace("{{" + item.Key + "}}", item.Value);
                }

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var client = new SendGridClient(_settings.SendGridApiKey);
                var from = new EmailAddress(_settings.SenderEmail, _settings.SenderName);
                var toEmail = new EmailAddress(to);

                var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, plainTextContent: null, htmlContent: body);
                var response = await client.SendEmailAsync(msg);

                if ((int)response.StatusCode >= 400)
                {
                    var error = await response.Body.ReadAsStringAsync();
                    throw new Exception($"SendGrid error: {error}");
                }

                _logger.LogInformation("SendGrid email sent to {Email}. Status code: {StatusCode}", to, response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }

        }
    }
}
