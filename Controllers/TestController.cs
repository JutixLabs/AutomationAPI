using AutomationAPI.DATA;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;
using AutomationAPI.SERVICES.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly GmailIntegrationService _gmailService;
        private readonly SlackService _slackService;
        private readonly GitHubService _gitHubService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISecretProtector _secretProtector;
        public TestController(
            AppDbContext context,
            GmailIntegrationService gmailService,
            SlackService slackService,
            GitHubService gitHubService,
            IHttpContextAccessor httpContextAccessor,
            ISecretProtector secretProtector)
        {
            _context = context;
            _gmailService = gmailService;
            _slackService = slackService;
            _gitHubService = gitHubService;
            _httpContextAccessor = httpContextAccessor;
            _secretProtector = secretProtector;
        }


        [Authorize]
        [HttpPost("gmail")]
        public async Task<IActionResult> TestGmail()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            var gmail = await _context.ConnectedApps
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.Provider == "google");

            if (gmail == null)
                return BadRequest(
                    "Gmail not connected");

            await _gmailService.SendEmail(
                _secretProtector.Unprotect(gmail.AccessToken),
                "kennethchukwuyemokonkwo@gmail.com",
                "Automation Test",
                "Your automation platform works!"
            );

            return Ok(new
            {
                success = true
            });
        }

        [Authorize]
        [HttpPost("slack")]
        public async Task<IActionResult> TestSlack()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            var slack =
                _context.ConnectedApps
                    .FirstOrDefault(x =>
                        x.UserId == userId &&
                        x.Provider == "slack");

            if (slack == null)
                return BadRequest(
                    "Slack not connected");

            if (string.IsNullOrEmpty(slack.AccessToken))
                return BadRequest(
                    "Slack access token is missing");

            await _slackService.SendMessage(
                _secretProtector.Unprotect(slack.AccessToken),
                "#general",
                "Automation platform test message 🚀"
            );

            return Ok("Message sent.");
        }

        [Authorize]
        [HttpPost("github")]
        public async Task<IActionResult> TestGitHub()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            var github =
                _context.ConnectedApps
                    .FirstOrDefault(x =>
                        x.UserId == userId &&
                        x.Provider == "github");
            if (github == null)
                return BadRequest(
                    "GitHub not connected");
            if (string.IsNullOrEmpty(github.AccessToken))
                return BadRequest(
                    "GitHub access token is missing");
            await _gitHubService.GetRepositories(
                _secretProtector.Unprotect(github.AccessToken)
            );

            return Ok("GitHub repositories retrieved successfully.");
        }
    }
}
