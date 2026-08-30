//using AutomationAPI.MODEL.Interface;

//namespace AutomationAPI.SERVICES
//{
//    public class IntegrationProviderResolver
//    {
//        private readonly IEnumerable<IIntegrationProvider> _providers;
//        public IntegrationProviderResolver(IEnumerable<IIntegrationProvider> providers)
//        {
//            _providers = providers;
//        }

//        public IIntegrationProvider Resolver(string provider)
//        {
//            return _providers.First(x => x.Provider.ToLower() == provider.ToLower());
//        }
//    }
//}
