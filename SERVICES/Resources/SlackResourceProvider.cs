using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;

namespace AutomationAPI.SERVICES.Resources
{
    public class SlackResourceProvider : IResourceProvider
    {
        private readonly ISlackProvider _slackProvider;
        public SlackResourceProvider(ISlackProvider slackProvider)
        {
            _slackProvider = slackProvider;
        }
        public async Task<List<ResourceOptionDto>> GetResourcesAsync(string resourceType, Dictionary<string, string> filters = null)
        {
            if (resourceType != "channel")
                return new();

            var channels =
                await _slackProvider
                    .GetChannelsAsync();

            return channels.Select(x =>
                new ResourceOptionDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();
        }

        public string ProviderName() => "slack";
    }
}
