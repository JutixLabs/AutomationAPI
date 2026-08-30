using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;

namespace AutomationAPI.SERVICES.Resources
{
    public class DiscordResourceProvider : IResourceProvider
    {
        private readonly IDiscordProvider _provider;
        public DiscordResourceProvider(IDiscordProvider provider)
        {
            _provider = provider;
        }

        public async Task<List<ResourceOptionDto>> GetResourcesAsync(string resourceType, Dictionary<string, string> filters = null)
        {
            switch (resourceType)
            {
                case "server":
                    return (await _provider.GetServersAsync()).Select(x => new ResourceOptionDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                    }).ToList();

                case "channel":
                    if (filters == null || !filters.TryGetValue("serverId", out var serverId))
                    {
                        return new();
                    }

                    return (await _provider.GetChannelsAsync(serverId)).Select(x => new ResourceOptionDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    }).ToList();

                default: return new();
            }
        }

        public string ProviderName() => "discord";
    }
}
