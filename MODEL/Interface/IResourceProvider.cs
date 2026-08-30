using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.MODEL.Interface
{
    public interface IResourceProvider
    {
        string ProviderName();
        Task<List<ResourceOptionDto>> GetResourcesAsync(string resourceType, Dictionary<string, string> filters = null);
    }
}
