using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.MODEL.Interface
{
    public interface IMetadataService
    {
        List<ProviderMetadataDTO> GetProviders();
        List<ActionMetadataDto> GetActions(string provider);
        List<ResourceMetadataDto> GetResources(string provider);
    }
}
