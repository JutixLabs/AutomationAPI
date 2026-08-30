using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Providers;

namespace AutomationAPI.SERVICES
{
    public class ResourceProviderResolver : IResourceProviderResolver
    {
        private readonly IEnumerable<IResourceProvider> _provider;
        public ResourceProviderResolver(IEnumerable<IResourceProvider> provider)
        {
            _provider = provider;
        }

        public IResourceProvider Resolve(string provider)
        {
            return _provider.First(x =>
                x.ProviderName()
                .Equals(provider, StringComparison.OrdinalIgnoreCase));
        }
    }
}
