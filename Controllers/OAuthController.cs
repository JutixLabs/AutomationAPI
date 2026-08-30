
using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;
using AutomationAPI.SERVICES.Persistence;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid.Helpers.Mail;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly IOAuthService _oauthService;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OAuthController> _logger;
        private readonly GmailIntegrationService _gmailService;
        private readonly ISecretProtector _secretProtector;
        public OAuthController(IOAuthService oauthService, AppDbContext dbContext, IConfiguration config,
            IHttpContextAccessor httpContextAccessor, ILogger<OAuthController> logger, GmailIntegrationService gmailService,
            ISecretProtector secretProtector)
        {
            _oauthService = oauthService;
            _dbContext = dbContext;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _gmailService = gmailService;
            _secretProtector = secretProtector;
        }

        [Authorize]
        [HttpGet("google")]
        public async Task<IActionResult> Google()
        {
            var result = _oauthService.Google();
            return Ok(new { result });
        }

        [AllowAnonymous]
        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback(string? code, string? state, string? error)
        {
            var token = await _oauthService.GoogleCallBack(code);
            //var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            //_logger.LogInformation("[USER_ID] Extracted userId {UserId} from token during Google callback.", userId);
            //if (string.IsNullOrEmpty(userId))
            //{
            //    _logger.LogError("[ERROR] Could not resolve userId from token during Google callback.");
            //    return Unauthorized("User identity could not be resolved.");
            //}
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("[ERROR] Google OAuth returned error: {Error}", error);
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=" + error);
            }
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError("[ERROR] Google OAuth callback received no code and no error.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=no_code");
            }
            var userId = state;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("[ERROR] State param is empty — cannot identify user.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=no_state");
            }

            _logger.LogInformation("[INFO] Received OAuth callback for user {UserId} with provider gmail", userId);

            var profile = await _gmailService.GetProfileAsync(token.AccessToken);
            try
            {
                var existing = await _dbContext.ConnectedApps
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == "google");

                if (existing != null)
                {
                    _logger.LogInformation("[INFO] Updating existing ConnectApp for user {UserId}", userId);

                    existing.AccessToken = _secretProtector.Protect(token.AccessToken);

                    // Google omits refresh token after first grant — only overwrite if a new one came in
                    if (!string.IsNullOrEmpty(token.RefreshToken))
                        existing.RefreshToken = token.RefreshToken;

                    existing.ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn ?? 3600);

                    // Explicitly mark as modified so EF Core change tracker picks it up
                    _dbContext.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    _logger.LogInformation("[INFO] Creating new ConnectApp for user {UserId}", userId);

                    var app = new ConnectedApp
                    {
                        UserId = userId,
                        Provider = "google",
                        AccessToken = _secretProtector.Protect(token.AccessToken),
                        RefreshToken = _secretProtector.Protect(token.RefreshToken),
                        ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn ?? 3600),
                        ExternalAccountId = profile.Id,
                        ExternalAccountEmail = profile.Email,
                        MetaDataJson = JsonConvert.SerializeObject(profile)
                    };

                    await _dbContext.ConnectedApps.AddAsync(app);
                }

                int rows = await _dbContext.SaveChangesAsync();
                _logger.LogInformation("[INFO] SaveChangesAsync affected {Rows} row(s) for user {UserId}", rows, userId);

                if (rows == 0)
                    _logger.LogWarning("[WARN] SaveChangesAsync reported 0 rows affected — data may not have been persisted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Failed to save ConnectApp for user {UserId}", userId);
                return StatusCode(500, "Failed to store OAuth credentials.");
            }

            // In GoogleCallback, change the redirect to:
            return Redirect("https://jutix-automation-api.vercel.app/myapps?connected=gmail");
        }

        [Authorize]
        [HttpGet("slack")]
        public async Task<IActionResult> Slack()
        {
            var result = _oauthService.Slack();
            return Ok(new { result });
        }

        [HttpGet("slack/callback")]
        public async Task<IActionResult> SlackCallback(string code, string state)
        {
            try
            {
                var userId = state;
                await _oauthService.SlackCallback(code, userId);

                return Redirect("https://jutix-automation-api.vercel.app/myapps?connected=slack");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Exception during Slack OAuth callback.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=callback_failed");
            }
        }

        [Authorize]
        [HttpGet("github")]
        public async Task<IActionResult> Github()
        {
            var result = _oauthService.GitHub();
            return Ok(new { result });
        }

        [HttpGet("github/callback")]
        public async Task<IActionResult> GithubCallback(string code, string state)
        {
            try
            {
                var userId = state;
                await _oauthService.GitHubCallback(code, userId);
                return Redirect("https://jutix-automation-api.vercel.app/myapps?connected=github");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Exception during GitHub OAuth callback.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=callback_failed");
            }
        }

        [Authorize]
        [HttpGet("discord")]
        public async Task<IActionResult> Discord()
        {
            var result = _oauthService.Discord();
            return Ok(new { result });
        }

        [AllowAnonymous]
        [HttpGet("discord-callback")]
        public async Task<IActionResult> DiscordCallback(string? code, string? state, string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("[ERROR] Discord OAuth returned error: {Error}", error);
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=" + error);
            }

            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError("[ERROR] Discord OAuth callback received no code and no error.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=no_code");
            }

            var userId = state;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("[ERROR] State param is empty — cannot identify user.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=no_state");
            }

            try
            {
                await _oauthService.DiscordCallbackAsync(code, userId);
                return Redirect("https://jutix-automation-api.vercel.app/myapps?connected=discord");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Exception during Discord OAuth callback.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=callback_failed");
            }
        }

        [Authorize]
        [HttpGet("notion")]
        public async Task<IActionResult> Notion()
        {
            var result = _oauthService.Notion();
            return Ok(new { result });
        }

        [Authorize]
        [HttpGet("notion-callback")]
        public async Task<IActionResult> NotionCallback(string? code, string? state, string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("[ERROR] Notion OAuth returned error: {Error}", error);
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=" + error);
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogError("[ERROR] Notion OAuth callback missing code or state.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=no_code");
            }

            try
            {
                await _oauthService.NotionCallbackAsync(code, state);
                return Redirect("https://jutix-automation-api.vercel.app/myapps?connected=notion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Exception during Notion OAuth callback.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=callback_failed");
            }
        }

        [Authorize]
        [HttpGet("trello")]
        public async Task<IActionResult> Trello()
        {
            var result = _oauthService.Trello();
            return Ok(new { result });
        }

        [Authorize]
        [HttpPost("trello/connect")]
        public async Task<IActionResult> ConnectTrello([FromBody] TrelloConnectRequest request)
        {
            if (string.IsNullOrEmpty(request?.Token))
                return BadRequest("Missing Trello token.");

            var userId = User.GetLoggedInUserId();

            try
            {
                await _oauthService.ConnectTrelloAsync(userId, request.Token);
                return Ok(new { connected = "trello" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Exception connecting Trello.");
                return BadRequest("Could not connect Trello.");
            }
        }

        [Authorize]
        [HttpPost("trello/watch-board")]
        public async Task<IActionResult> WatchTrelloBoard([FromBody] WatchBoardRequest request, [FromServices] ITrelloProvider trelloProvider, [FromServices] AppDbContext dbContext, [FromServices] IConfiguration config)
        {
            if (string.IsNullOrEmpty(request?.BoardId))
                return BadRequest("Missing boardId.");

            var userId = User.GetLoggedInUserId();
            var callbackUrl = config["AppBaseUrl"] + "/api/Webhooks/trello";

            try
            {
                var webhookId = await trelloProvider.CreateWebhookAsync(userId, request.BoardId, callbackUrl);

                dbContext.TrelloWatchedBoards.Add(new MODEL.Entity.TrelloWatchedBoard
                {
                    UserId = userId,
                    BoardId = request.BoardId,
                    BoardName = request.BoardName,
                    TrelloWebhookId = webhookId,
                });
                await dbContext.SaveChangesAsync();

                return Ok(new { watching = request.BoardId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Failed to create Trello webhook.");
                return BadRequest("Could not watch board — check that your server is publicly reachable.");
            }
        }

        [Authorize]
        [HttpGet("trello/watched-boards")]
        public async Task<IActionResult> GetWatchedTrelloBoards([FromServices] AppDbContext dbContext)
        {
            var userId = User.GetLoggedInUserId();
            var boards = await dbContext.TrelloWatchedBoards.Where(b => b.UserId == userId).ToListAsync();
            return Ok(boards);
        }

        [Authorize]
        [HttpDelete("trello/watch-board/{id}")]
        public async Task<IActionResult> UnwatchTrelloBoard(int id, [FromServices] ITrelloProvider trelloProvider, [FromServices] AppDbContext dbContext)
        {
            var userId = User.GetLoggedInUserId();
            var watched = await dbContext.TrelloWatchedBoards.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (watched == null)
                return NotFound();

            try
            {
                await trelloProvider.DeleteWebhookAsync(userId, watched.TrelloWebhookId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Failed to delete Trello webhook — removing local record anyway.");
            }

            dbContext.TrelloWatchedBoards.Remove(watched);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpPost("discord/watch-channel")]
        public async Task<IActionResult> WatchDiscordChannel([FromBody] WatchChannelRequest request, [FromServices] AppDbContext dbContext)
        {
            if (string.IsNullOrEmpty(request?.ChannelId))
                return BadRequest("Missing channelId.");

            var userId = User.GetLoggedInUserId();

            dbContext.DiscordWatchedChannels.Add(new MODEL.Entity.DiscordWatchedChannel
            {
                UserId = userId,
                ServerId = request.ServerId,
                ServerName = request.ServerName,
                ChannelId = request.ChannelId,
                ChannelName = request.ChannelName,
            });
            await dbContext.SaveChangesAsync();

            return Ok(new { watching = request.ChannelId });
        }

        [Authorize]
        [HttpGet("discord/watched-channels")]
        public async Task<IActionResult> GetWatchedDiscordChannels([FromServices] AppDbContext dbContext)
        {
            var userId = User.GetLoggedInUserId();
            var channels = await dbContext.DiscordWatchedChannels.Where(c => c.UserId == userId).ToListAsync();
            return Ok(channels);
        }

        [Authorize]
        [HttpDelete("discord/watch-channel/{id}")]
        public async Task<IActionResult> UnwatchDiscordChannel(int id, [FromServices] AppDbContext dbContext)
        {
            var userId = User.GetLoggedInUserId();
            var watched = await dbContext.DiscordWatchedChannels.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (watched == null)
                return NotFound();

            dbContext.DiscordWatchedChannels.Remove(watched);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpGet("stripe")]
        public async Task<IActionResult> Stripe()
        {
            var result = _oauthService.Stripe();
            return Ok(new { result });
        }

        [AllowAnonymous]
        [HttpGet("stripe-callback")]
        public async Task<IActionResult> StripeCallback(string? code, string? state, string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("[ERROR] Stripe OAuth returned error: {Error}", error);
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=" + error);
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogError("[ERROR] Stripe OAuth callback missing code or state.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=no_code");
            }

            try
            {
                await _oauthService.StripeCallbackAsync(code, state);
                return Redirect("https://jutix-automation-api.vercel.app/myapps?connected=stripe");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Exception during Stripe OAuth callback.");
                return Redirect("https://jutix-automation-api.vercel.app/myapps?error=callback_failed");
            }
        }
    }
}