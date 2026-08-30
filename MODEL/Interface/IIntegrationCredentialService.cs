using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IIntegrationCredentialService
    {
        Task<string> GetCredentialAsync(string provider);
        Task SaveCredentialAsync(IntegrationCredential credential);
        Task<bool> ExistsAsync(string provider);
    }
}
