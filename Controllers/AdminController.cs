using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IIntegrationCredentialService _credentials;
        private readonly IConfiguration _config;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IIntegrationCredentialService credentials, IConfiguration config, ILogger<AdminController> logger)
        {
            _credentials = credentials;
            _config = config;
            _logger = logger;
        }

        private bool IsAuthorizedAdmin()
        {
            if (!Request.Headers.TryGetValue("X-Admin-Key", out var providedKey))
                return false;

            var expectedKey = _config["AdminApiKey"];
            if (string.IsNullOrEmpty(expectedKey))
                return false;

            // Constant-time comparison so response timing can't leak the correct key
            // byte-by-byte to someone probing this endpoint.
            var provided = Encoding.UTF8.GetBytes(providedKey.ToString());
            var expected = Encoding.UTF8.GetBytes(expectedKey);

            if (provided.Length != expected.Length)
                return false;

            return CryptographicOperations.FixedTimeEquals(provided, expected);
        }

        [HttpPost("credentials")]
        public async Task<IActionResult> SaveCredential([FromBody] SaveCredentialRequest request)
        {
            if (!IsAuthorizedAdmin())
                return Unauthorized();

            if (string.IsNullOrEmpty(request?.Provider) || string.IsNullOrEmpty(request.Value))
                return BadRequest("Provider and Value are required.");

            await _credentials.SaveCredentialAsync(new IntegrationCredential
            {
                Provider = request.Provider,
                Name = request.Name ?? request.Provider,
                CredentialType = request.CredentialType ?? "bot_token",
                Value = request.Value,
                IsActive = true,
            });

            _logger.LogInformation("[INFO] Admin saved credential for provider: {Provider}", request.Provider);

            // Never echo the value back, even to an authenticated admin caller —
            // one less place a secret could end up in a log or browser history.
            return Ok(new { saved = request.Provider });
        }

        [HttpGet("credentials/{provider}/exists")]
        public async Task<IActionResult> CredentialExists(string provider)
        {
            if (!IsAuthorizedAdmin())
                return Unauthorized();

            var exists = await _credentials.ExistsAsync(provider);
            return Ok(new { provider, exists });
        }
    }
}
