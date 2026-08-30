using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;

namespace AutomationAPI.SERVICES.Resources
{
    public class StripeResourceProvider : IResourceProvider
    {
        private readonly IStripeProvider _stripeProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StripeResourceProvider(IStripeProvider stripeProvider, IHttpContextAccessor httpContextAccessor)
        {
            _stripeProvider = stripeProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public string ProviderName() => "stripe";

        public async Task<List<ResourceOptionDto>> GetResourcesAsync(string resourceType, Dictionary<string, string> filters = null)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            switch (resourceType)
            {
                case "customer":
                    {
                        var customers = await _stripeProvider.GetCustomersAsync(userId);
                        return customers.Select(c => new ResourceOptionDto
                        {
                            Id = c.Id,
                            Name = string.IsNullOrEmpty(c.Name) ? c.Email ?? c.Id : c.Name
                        }).ToList();
                    }

                default:
                    return new List<ResourceOptionDto>();
            }
        }
    }
}