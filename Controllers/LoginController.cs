using AutomationAPI.DATA;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.MODEL.LoginDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IJwtGenerator _jwtGenerator;
        public LoginController(AppDbContext dbContext, IJwtGenerator jwtGenerator)
        {
            _dbContext = dbContext;
            _jwtGenerator = jwtGenerator;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Login(LoginDTO model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Please Provide Email and Password");

                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
                if (user == null) return NotFound("User not found. Check email address.");

                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                    return Unauthorized("Invalid password.");

                //if (!user.IsEmailVerified)
                //    return BadRequest("Please verify your email first");

                var token = await _jwtGenerator.GenerateJwtToken(user);
                if (token == null) return BadRequest();

                return Ok(new { token = token });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error]: {ex.Message}");
                throw;
            }
        }
    }
}
