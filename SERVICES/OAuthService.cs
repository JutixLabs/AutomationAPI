using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Util.Store;
using Hangfire.States;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace AutomationAPI.SERVICES
{
    public class OAuthService : IOAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OAuthService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SlackService _slackService;
        private readonly GitHubService _githubService;
        private readonly ISecretProtector _secretProtector;
        //private readonly DiscordS
        public OAuthService(AppDbContext dbContext, IConfiguration config, IHttpContextAccessor httpContextAccessor, 
            ILogger<OAuthService> logger, IHttpClientFactory httpClientFactory, SlackService slackService, GitHubService gitHubService,
            ISecretProtector secretProtector)
        {
            _dbContext = dbContext;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _slackService = slackService;
            _githubService = gitHubService;
            _secretProtector = secretProtector;

        }
        public string Google()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            try
            {
                var clientId =
                    _config["GoogleOAuth:ClientId"];

                var redirectUri =
                    _config["GoogleOAuth:RedirectUri"];

                var scope =
                    "openid email profile " +
                    "https://www.googleapis.com/auth/gmail.send " +
                    "https://www.googleapis.com/auth/spreadsheets.readonly";

                var authUrl =
                    $"https://accounts.google.com/o/oauth2/v2/auth" +
                    $"?client_id={clientId}" +
                    $"&redirect_uri={redirectUri}" +
                    $"&response_type=code" +
                    $"&scope={Uri.EscapeDataString(scope)}" +
                    $"&state={userId}" +
                    $"&access_type=offline" +
                    $"&prompt=consent";

                _logger.LogInformation("[INFO] Generated Google OAuth URL for user: {UserId}. [URL]: {AuthUrl}", userId, authUrl);

                return authUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[EEROR]: {ex.Message}");
                throw;
            }
        }

        public async Task<TokenResponse> GoogleCallBack(string code)
        {
            try
            {
                var clientId = _config["GoogleOAuth:ClientId"]
                    ?? throw new InvalidOperationException("GoogleOAuth:ClientId is not configured.");

                var clientSecret = _config["GoogleOAuth:ClientSecret"]
                    ?? throw new InvalidOperationException("GoogleOAuth:ClientSecret is not configured.");

                var redirectUri = _config["GoogleOAuth:RedirectUri"]
                    ?? throw new InvalidOperationException("GoogleOAuth:RedirectUri is not configured.");

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
                {
                    _logger.LogError("[ERROR] Google OAuth configuration is incomplete. Check ClientId, ClientSecret, RedirectUri.");
                    throw new InvalidOperationException("Google OAuth configuration is missing.");
                }

                using var client = new HttpClient();

                var values = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(
                    "https://oauth2.googleapis.com/token",
                    content
                );

                // Log raw response before EnsureSuccessStatusCode so you see Google's error body
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("[ERROR] Google token exchange failed. Status: {StatusCode}, Body: {Body}",
                        response.StatusCode, json);
                    throw new HttpRequestException($"Google token exchange failed: {response.StatusCode}");
                }

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true  // handles snake_case <-> PascalCase safely
                };

                var token = System.Text.Json.JsonSerializer.Deserialize<GoogleTokenResponse>(json, options);

                if (token == null || string.IsNullOrEmpty(token.access_token))
                {
                    _logger.LogError("[ERROR] Google token deserialization returned null or empty access_token. Raw JSON: {Json}", json);
                    throw new InvalidOperationException("Failed to deserialize Google token response.");
                }

                if (string.IsNullOrEmpty(token.refresh_token))
                    _logger.LogWarning("[WARN] No refresh_token returned by Google. User may need to re-authorize with prompt=consent.");

                _logger.LogInformation("[INFO] Google OAuth token exchange successful. TokenType: {TokenType}, ExpiresIn: {ExpiresIn}",
                    token.token_type, token.expires_in);

                return new TokenResponse
                {
                    AccessToken = token.access_token,
                    RefreshToken = token.refresh_token,
                    ExpiresIn = token.expires_in,
                    TokenType = token.token_type
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] GoogleCallBack failed during token exchange.");
                throw;
            }
        }

        public string Slack()
        {
            try
            {
                var clientId =
                    _config["SlackOAuth:ClientId"];

                var redirectUri =
                    "https://kennethokonkwo-002-site2.itempurl.com/api/OAuth/slack/callback";

                var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
                var url =
                    "https://slack.com/oauth/v2/authorize" +
                    $"?client_id={clientId}" +
                    $"&scope=chat:write,channels:read" +
                    $"&redirect_uri={redirectUri}" +
                    $"&user_scope=identity.basic,identity.email,identity.team" +
                    $"&state={userId}";

                _logger.LogInformation("[INFO] Generated Slack OAuth URL: {Url}", url);

                return url;
            }
            catch(Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task SlackCallback(string code, string userId)
        {
            try
            {
                var clientId =
                    _config["SlackOAuth:ClientId"]
                    ?? throw new InvalidOperationException("SlackOAuth:ClientId Is Not Configured.");

                var clientSecret =
                    _config["SlackOAuth:ClientSecret"]
                    ?? throw new InvalidOperationException("SlackOAuth:ClientSecret Is Not Configured.");

                var redirectUri =
                    "https://kennethokonkwo-002-site2.itempurl.com/api/OAuth/slack/callback";

                var client = _httpClientFactory.CreateClient();

                var response =
                    await client.PostAsync(
                        "https://slack.com/api/oauth.v2.access",
                        new FormUrlEncodedContent(
                            new Dictionary<string, string>
                            {
                                { "client_id", clientId },
                                { "client_secret", clientSecret },
                                { "code", code },
                                { "redirect_uri", redirectUri }
                            }));

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine(json);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("[ERROR] Slack token exchange failed. Status: {StatusCode}, Body: {Body}",
                        response.StatusCode, json);
                    throw new HttpRequestException($"Slack token exchange failed: {response.StatusCode}");
                }

                dynamic result =
                    JsonConvert.DeserializeObject(json);

                if (result.ok != true)
                {
                    throw new Exception(
                        result.error.ToString());
                }

                var botAccessToken =
                    (string)result.access_token;
                var userToken = (string)(result.authed_user.access_token
                    ?? throw new Exception("No user access token returned by Slack."));

                var profile = await _slackService.GetProfileAsync(userToken);

                if (profile?.User == null)
                    throw new InvalidOperationException("Slack profile missing user object.");

                _logger.LogInformation("[INFO] Slack OAuth token exchange successful for user: {UserId}.", userId);
                
                var existing =
                    _dbContext.ConnectedApps
                        .FirstOrDefault(x =>
                            x.UserId == userId &&
                            x.Provider == "slack");

                if (existing != null)
                {
                    existing.AccessToken = _secretProtector.Protect(botAccessToken);
                    existing.RefreshToken = _secretProtector.Protect(userToken);        
                    existing.IsActive = true;
                    existing.ConnectedAt = DateTime.UtcNow;
                    existing.ExpiresAt = DateTime.UtcNow.AddSeconds(3600);
                    existing.ExternalAccountId = profile.User.Id;
                    existing.ExternalAccountEmail = profile.User.Email ?? "";
                    existing.MetaDataJson = JsonConvert.SerializeObject(profile);
                    existing.LastSyncCursor = "";
                }
                else
                {
                    _dbContext.ConnectedApps.Add(
                        new ConnectedApp
                        {
                            UserId = userId,
                            Provider = "slack",
                            RefreshToken = _secretProtector.Protect(userToken),
                            AccessToken = _secretProtector.Protect(botAccessToken),
                            IsActive = true,
                            ConnectedAt = DateTime.UtcNow,
                            ExpiresAt = DateTime.UtcNow.AddSeconds(3600),
                            ExternalAccountId = profile.User.Id,
                            ExternalAccountEmail = profile.User.Email ?? "",
                            MetaDataJson = JsonConvert.SerializeObject(profile),
                            LastSyncCursor = ""
                        });
                    _logger.LogInformation("[INFO] New Slack ConnectApp created for user: {UserId}.", userId);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public string GitHub()
        {
            var clientId = _config["GitHubOAuth:ClientId"];

            var redirectUri = "https://kennethokonkwo-002-site2.itempurl.com/api/OAuth/github/callback";

            var scopes = new[] { "repo", "user:email" };
            var scope = Uri.EscapeDataString(string.Join(" ", scopes));

            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            var url =
                "https://github.com/login/oauth/authorize" +
                $"?client_id={clientId}" +
                $"&redirect_uri={redirectUri}" +
                $"&scope={scope}" +
                $"&state={userId}";

            _logger.LogInformation("[INFO] Generated GitHub OAuth URL: {Url}", url);

            return url;
        }

        public async Task GitHubCallback(string code, string userId)
        {
            var clientId = _config["GitHubOAuth:ClientId"]
                ?? throw new InvalidOperationException("GitHubOAuth:ClientId Is Not Configured.");

            var clientSecret = _config["GitHubOAuth:ClientSecret"]
                ?? throw new InvalidOperationException("GitHubOAuth:ClientId Is Not Configured."); ;

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Accept
                .Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(
                    "https://github.com/login/oauth/access_token",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            { "client_id", clientId },
                            { "client_secret", clientSecret },
                            { "code", code  }
                        }));

            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine(json);

            dynamic result = JsonConvert.DeserializeObject(json);

            if (result.access_token == null)
                throw new Exception((string)(result.error ?? "Github token exchange failed."));

            var accessToken =
                    (string)result.access_token;

            var profile = await _githubService.GetProfileAsync(accessToken);

            if (profile == null)
                throw new InvalidOperationException("Github profile missing user object.");

            var email = profile.Email ?? await _githubService.GetPrimaryEmail(accessToken);

            var existing =
                    _dbContext.ConnectedApps
                        .FirstOrDefault(x =>
                            x.UserId == userId &&
                            x.Provider == "github");
            
            if (existing != null)
            {
                existing.AccessToken = _secretProtector.Protect(accessToken);
                existing.RefreshToken = "";
                existing.IsActive = true;
                existing.ConnectedAt = DateTime.UtcNow;
                existing.ExpiresAt = DateTime.UtcNow.AddYears(1);
                existing.ExternalAccountId = profile.Id.ToString();
                existing.ExternalAccountEmail = email;
                existing.MetaDataJson = JsonConvert.SerializeObject(profile);
                existing.LastSyncCursor = "";
            }
            else
            {
                _dbContext.ConnectedApps.Add(
                    new ConnectedApp
                    {
                        UserId = userId,
                        Provider = "github",
                        AccessToken = _secretProtector.Protect(accessToken),
                        RefreshToken = "",
                        IsActive = true,
                        ConnectedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddYears(1),
                        ExternalAccountId = profile.Id.ToString(),
                        ExternalAccountEmail = email,
                        MetaDataJson = JsonConvert.SerializeObject(profile),
                        LastSyncCursor = ""
                    });

                _logger.LogInformation("[INFO] New GitHub ConnectedApp created for user: {UserId}.", userId);
            }

            await _dbContext.SaveChangesAsync();
        }

        public string Discord()
        {
            var clientId = _config["DiscordOAuth:ClientId"];

            var redirect = Uri.EscapeDataString(_config["DiscordOAuth:RedirectUri"]);

            var scopes = Uri.EscapeDataString("identify email guilds");

            var url =
                $"https://discord.com/api/oauth2/authorize" +
                $"?client_id={clientId}" +
                $"&redirect_uri={redirect}" +
                $"&response_type=code" +
                $"&scope={scopes}";

            return url;
        }

        public async Task DiscordCallbackAsync(string code, string userId)
        {
            var clientId = _config["DiscordOAuth:ClientId"]
                ?? throw new InvalidOperationException("DiscordOAuth:ClientId Is Not Configured.");

            var clientSecret = _config["DiscordOAuth:ClientSecret"]
                ?? throw new InvalidOperationException("DiscordOAuth:ClientId Is Not Configured."); ;

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Accept
                .Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(
                    "https://discord.com/api/oauth2/token",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            { "client_id", clientId },
                            { "client_secret", clientSecret },
                            { "code", code  }
                        }));

            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine(json);

            dynamic result = JsonConvert.DeserializeObject(json);

            if (result.access_token == null)
                throw new Exception((string)(result.error ?? "Discord token exchange failed."));

            var accessToken =
                    (string)result.access_token;

            var profile = await GetDiscordProfileAsync(accessToken);

            if (profile == null)
                throw new InvalidOperationException("Discord profile missing user object.");

            var email = profile.Email;

            var existing =
                    _dbContext.ConnectedApps
                        .FirstOrDefault(x =>
                            x.UserId == userId &&
                            x.Provider == "discord");

            if (existing != null)
            {
                existing.AccessToken = _secretProtector.Protect(accessToken);
                existing.RefreshToken = "";
                existing.IsActive = true;
                existing.ConnectedAt = DateTime.UtcNow;
                existing.ExpiresAt = DateTime.UtcNow.AddYears(1);
                existing.ExternalAccountId = profile.Id.ToString();
                existing.ExternalAccountEmail = email;
                existing.MetaDataJson = JsonConvert.SerializeObject(profile);
                existing.LastSyncCursor = "";
            }
            else
            {
                _dbContext.ConnectedApps.Add(
                    new ConnectedApp
                    {
                        UserId = userId,
                        Provider = "discord",
                        AccessToken = _secretProtector.Protect(accessToken),
                        RefreshToken = "",
                        IsActive = true,
                        ConnectedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddYears(1),
                        ExternalAccountId = profile.Id.ToString(),
                        ExternalAccountEmail = email,
                        MetaDataJson = JsonConvert.SerializeObject(profile),
                        LastSyncCursor = ""
                    });

                _logger.LogInformation("[INFO] New Discord ConnectedApp created for user: {UserId}.", userId);
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task<DiscordProfileDto> GetDiscordProfileAsync(string accessToken)
        {
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://discord.com/api/users/@me");

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[ERROR] Discord profile fetch failed. Status: {StatusCode}, Body: {Body}",
                    response.StatusCode, json);
                throw new HttpRequestException($"Discord profile fetch failed: {response.StatusCode}");
            }

            return JsonConvert.DeserializeObject<DiscordProfileDto>(json);
        }

        public string Notion()
        {
            var clientId = _config["NotionOAuth:ClientId"];
            var redirectUri = _config["NotionOAuth:RedirectUri"];
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            return "https://api.notion.com/v1/oauth/authorize" +
                   $"?owner=user&client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&response_type=code&state={userId}";
        }

        public async Task NotionCallbackAsync(string code, string userId)
        {
            var clientId = _config["NotionOAuth:ClientId"];
            var clientSecret = _config["NotionOAuth:ClientSecret"];
            var redirectUri = _config["NotionOAuth:RedirectUri"];

            var client = _httpClientFactory.CreateClient();

            var basicAuth = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", basicAuth);

            var response = await client.PostAsJsonAsync(
                "https://api.notion.com/v1/oauth/token",
                new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri = redirectUri
                });

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[ERROR] Notion token exchange failed: {Body}", json);
                throw new Exception("Notion token exchange failed.");
            }

            var token = JsonConvert.DeserializeObject<NotionTokenResponse>(json);

            var existing = await _dbContext.ConnectedApps
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == "notion");

            if (existing != null)
            {
                existing.AccessToken = _secretProtector.Protect(token.AccessToken);
                existing.ExternalAccountId = token.WorkspaceId;
                existing.IsActive = true;
                existing.MetaDataJson = JsonConvert.SerializeObject(token);
                _logger.LogInformation("[INFO] Notion ConnectedApp updated for user: {UserId}.", userId);
            }
            else
            {
                _dbContext.ConnectedApps.Add(new ConnectedApp
                {
                    UserId = userId,
                    Provider = "notion",
                    AccessToken = _secretProtector.Protect(token.AccessToken),
                    ExternalAccountId = token.WorkspaceId,
                    IsActive = true,
                    MetaDataJson = JsonConvert.SerializeObject(token)
                });
                _logger.LogInformation("[INFO] New Notion ConnectedApp created for user: {UserId}.", userId);
            }

            await _dbContext.SaveChangesAsync();
        }

        public string Trello()
        {
            var apiKey = _config["TrelloOAuth:ApiKey"];
            var redirectUri = _config["TrelloOAuth:RedirectUri"];

            return "https://trello.com/1/authorize" +
                   "?expiration=never&scope=read,write&response_type=token" +
                   "&name=JUTIX%20AutomationsAPI" +
                   $"&key={apiKey}" +
                   $"&return_url={Uri.EscapeDataString(redirectUri)}";
        }

        public async Task ConnectTrelloAsync(string userId, string token)
        {
            var apiKey = _config["TrelloOAuth:ApiKey"];

            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(
                $"https://api.trello.com/1/members/me?key={apiKey}&token={token}");

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[ERROR] Trello token validation failed: {Body}", json);
                throw new Exception("Trello token validation failed.");
            }

            var member = JsonConvert.DeserializeObject<TrelloMemberDto>(json);

            var existing = await _dbContext.ConnectedApps
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == "trello");

            if (existing != null)
            {
                existing.AccessToken = _secretProtector.Protect(token);
                existing.ExternalAccountId = member.Id;
                existing.IsActive = true;
                existing.MetaDataJson = JsonConvert.SerializeObject(member);
                _logger.LogInformation("[INFO] Trello ConnectedApp updated for user: {UserId}.", userId);
            }
            else
            {
                _dbContext.ConnectedApps.Add(new ConnectedApp
                {
                    UserId = userId,
                    Provider = "trello",
                    AccessToken = _secretProtector.Protect(token),
                    ExternalAccountId = member.Id,
                    IsActive = true,
                    MetaDataJson = JsonConvert.SerializeObject(member)
                });
                _logger.LogInformation("[INFO] New Trello ConnectedApp created for user: {UserId}.", userId);
            }

            await _dbContext.SaveChangesAsync();
        }

        public string Stripe()
        {
            var clientId = _config["StripeConnect:ClientId"];
            var redirectUri = _config["StripeConnect:RedirectUri"];
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            return "https://connect.stripe.com/oauth/authorize" +
                   $"?response_type=code&client_id={clientId}&scope=read_write" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&state={userId}";
        }

        public async Task StripeCallbackAsync(string code, string userId)
        {
            var secretKey = _config["StripeConnect:SecretKey"];

            var client = _httpClientFactory.CreateClient();

            var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secretKey}:"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", basicAuth);

            var form = new Dictionary<string, string>
            {
                { "code", code },
                { "grant_type", "authorization_code" }
            };

            var response = await client.PostAsync(
                "https://connect.stripe.com/oauth/token",
                new FormUrlEncodedContent(form));

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[ERROR] Stripe token exchange failed: {Body}", json);
                throw new Exception("Stripe token exchange failed.");
            }

            var token = JsonConvert.DeserializeObject<StripeTokenResponse>(json);

            var existing = await _dbContext.ConnectedApps
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == "stripe");

            if (existing != null)
            {
                existing.AccessToken = _secretProtector.Protect(token.AccessToken);
                existing.RefreshToken = _secretProtector.Protect(token.RefreshToken);
                existing.ExternalAccountId = token.StripeUserId;
                existing.IsActive = true;
                existing.MetaDataJson = JsonConvert.SerializeObject(token);
                _logger.LogInformation("[INFO] Stripe ConnectedApp updated for user: {UserId}.", userId);
            }
            else
            {
                _dbContext.ConnectedApps.Add(new ConnectedApp
                {
                    UserId = userId,
                    Provider = "stripe",
                    AccessToken = _secretProtector.Protect(token.AccessToken),
                    RefreshToken = _secretProtector.Protect(token.RefreshToken),
                    ExternalAccountId = token.StripeUserId,
                    IsActive = true,
                    MetaDataJson = JsonConvert.SerializeObject(token)
                });
                _logger.LogInformation("[INFO] New Stripe ConnectedApp created for user: {UserId}.", userId);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
} 