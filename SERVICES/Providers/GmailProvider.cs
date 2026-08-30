using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;

namespace AutomationAPI.SERVICES.Providers
{
    public class GmailProvider : IIntegrationProvider
    {
        private readonly GmailIntegrationService _gmailService;
        private readonly IVariableResolver _variableResolver;
        public GmailProvider(GmailIntegrationService gmailService, IVariableResolver variableResolver)
        {
            _gmailService = gmailService;
            _variableResolver = variableResolver;
        }
        public string Provider => "gmail";

        public async Task ExecuteAsync(ConnectedApp app, string action, Dictionary<string, object> payload)
        {
            switch (action)
            {
                case "send_email":
                    var to = _variableResolver.Resolve(
                        payload["to"].ToString(),
                        payload);

                    var subject = _variableResolver.Resolve(
                        payload["subject"].ToString(),
                        payload);

                    var body = _variableResolver.Resolve(
                        payload["body"].ToString(),
                        payload);

                    await _gmailService.SendEmail(
                        app.AccessToken,
                        to,
                        subject,
                        body);

                    break;
            }
        }
    }
}
