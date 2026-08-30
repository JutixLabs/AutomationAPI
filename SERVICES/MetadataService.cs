using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Metadata;

namespace AutomationAPI.SERVICES
{
    public class MetadataService : IMetadataService
    {
        public List<ActionMetadataDto> GetActions(string provider)
        {
            return ActionCatalog.GetActions(provider);
        }

        public List<ProviderMetadataDTO> GetProviders()
        {
            return ProviderCatalog.Providers;

        }

        public List<ResourceMetadataDto> GetResources(string provider)
        {
            return ResourceCatalog.GetResources(provider);
        }
    }
}
