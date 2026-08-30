using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AutomationAPI.SERVICES.Persistence
{
    public static class ClaimsPrincipalExtension
    {
        public static string GetLoggedInUserId(this ClaimsPrincipal user)
        {
            var userID =
                user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userID))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return userID;
        }
    }
}
