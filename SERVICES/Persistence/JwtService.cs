using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutomationAPI.SERVICES.Persistence
{
    public class JwtService : IJwtGenerator
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public JwtService(AppDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }
        public async Task<string> GenerateJwtToken(User userModel)
        {
            string audience = string.Empty;
            string issuer = string.Empty;
            byte[] key = null;

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userModel.Email);
            if (user == null || BCrypt.Net.BCrypt.Verify(userModel.PasswordHash, user.PasswordHash))
                throw new Exception("Invalid email or password.");

            audience = _configuration.GetValue<string>("JWT:Audience");
            issuer = _configuration.GetValue<string>("JWT:Issuer");
            key = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("JWT:Secret"));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Audience = audience,
                Issuer = issuer,
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.ID),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return jwt;
        }
    }
}
