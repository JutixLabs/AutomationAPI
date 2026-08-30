using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IUserService
    {
        Task<User> AddUserAsync(User model);
        Task<User> GetCurrentUserAsync();
        Task<User> UpdateProfileAsync(UpdateProfileRequest model);
        Task ChangePasswordAsync(ChangePasswordRequest model);
        Task<string> DeleteAccountAsync();
    }
}
