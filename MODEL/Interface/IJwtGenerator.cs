using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IJwtGenerator
    {
        Task<string> GenerateJwtToken(User userModel);

    }
}
