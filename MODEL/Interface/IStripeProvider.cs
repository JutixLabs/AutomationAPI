using AutomationAPI.MODEL.DTO;

namespace AutomationAPI.MODEL.Interface
{
    public interface IStripeProvider
    {
        Task<List<StripeCustomerDto>> GetCustomersAsync(string userId);
        Task CreateCustomerAsync(string userId, string email, string name);
        Task CreateRefundAsync(string userId, string chargeId);
    }
}
