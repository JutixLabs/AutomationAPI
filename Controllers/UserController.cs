using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser(User model)
        {
            var result = await _userService.AddUserAsync(model);
            return Ok(result);
        }

        [HttpGet("Me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {

            var result = await _userService.GetCurrentUserAsync();
            return Ok(result);
        }

        [HttpPut("Change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
        {
            var result = _userService.ChangePasswordAsync(model);
            return Ok($"[Password Changed]: {result}");
        }

        [HttpPut("Update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest model)
        {
            var result = await _userService.UpdateProfileAsync(model);
            return Ok(result);
        }

        [HttpDelete("Delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            var result = await _userService.DeleteAccountAsync();
            return Ok(result);
        }
    }
}
