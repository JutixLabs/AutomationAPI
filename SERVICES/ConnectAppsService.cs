using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;
using AutomationAPI.SERVICES.Secrets;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.SERVICES
{
    public class ConnectAppsService : IConnectAppsService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ConnectAppsService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISecretProtector _secretProtector;
        public ConnectAppsService(AppDbContext dbContext, ILogger<ConnectAppsService> logger, IHttpContextAccessor httpContextAccessor, ISecretProtector secretProtector)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _secretProtector = secretProtector;
        }

        public async Task<ConnectedApp> ConnectAppAsync(ConnectAppDTO model)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            try
            {
                var provider = model.Provider.ToLower().Trim();
                var validate = await _dbContext.ConnectedApps.AnyAsync(c => c.UserId == userId && c.Provider.ToLower().Trim() == provider);
                if (validate)
                    throw new Exception($"[ERROR] User {userId} has already connected with {provider}.");

                var connectApp = new ConnectedApp
                {
                    UserId = userId,
                    Provider = provider,
                    AccessToken = _secretProtector.Protect(model.AccessToken),
                    RefreshToken = _secretProtector.Protect(model.RefreshToken),
                    ExpiresAt = model.ExpiresAt,
                };

                if (connectApp.ExpiresAt <= DateTime.UtcNow)
                {
                    _dbContext.ConnectedApps.Remove(connectApp);
                    throw new Exception($"[ERROR] The access token for {provider} has already expired.");
                }

                await _dbContext.ConnectedApps.AddAsync(connectApp);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"[INFO] User {userId} connected with {provider} successfully.");

                return connectApp;
            }
            catch (Exception ex) 
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task<string> DeactivateAppAsync(int id)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            try
            {
                var validateApp = await _dbContext.ConnectedApps.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == id);
                if (validateApp == null)
                    throw new Exception($"[ERROR] User {userId} has not connected with app ID {id}.");

                validateApp.IsActive = false;
                _dbContext.ConnectedApps.Remove(validateApp);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"[INFO] User {userId} deactivated app ID {id} successfully.");
                return "Disconnected";
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetAccessTokenAsync(string provider)
        {
            await RefreshTokenIfNeededAsync(provider);

            var connection = await GetConnectionAsync(provider);

            return _secretProtector.Unprotect(connection.AccessToken);
        }

        public async Task<List<ConnectedApp>> GetConnectedAppsAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();
            try
            {
                var connectedApps = await _dbContext.ConnectedApps
                    .Where(c => c.UserId == userId)
                    .Select(c => new
                    {
                        c.Id,
                        c.Provider,
                        c.IsActive,
                        c.ConnectedAt,
                        c.ExternalAccountEmail,
                        c.ExternalAccountId,
                        c.MetaDataJson,
                        c.ExpiresAt,
                        c.LastSyncCursor
                    })
                    .ToListAsync();
                if (connectedApps == null || connectedApps.Count == 0)
                    throw new Exception($"[ERROR] User {userId} has not connected with any apps.");

                var list = new List<ConnectedApp>();
                foreach (var app in connectedApps)
                {
                    list.Add(new ConnectedApp
                    {
                        Id = app.Id,
                        Provider = app.Provider,
                        IsActive = app.IsActive,
                        ConnectedAt = app.ConnectedAt,
                        ExternalAccountEmail = app.ExternalAccountEmail,
                        ExternalAccountId = app.ExternalAccountId,
                        MetaDataJson = app.MetaDataJson,
                        ExpiresAt = app.ExpiresAt
                    });
                }

                _logger.LogInformation($"[INFO] User {userId} retrieved connected apps successfully.");
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task<ConnectedApp> GetConnectionAsync(string provider)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            var connection = await _dbContext.ConnectedApps.FirstOrDefaultAsync(x =>
                x.UserId == userId && x.Provider == provider && x.IsActive);

            if (connection == null)
                throw new Exception($"{provider} account not connected.");

            return connection;
        }

        public async Task RefreshTokenIfNeededAsync(string provider)
        {
            var connection = await GetConnectionAsync(provider);

            if (!connection.ExpiresAt.HasValue)
                return;

            if (connection.ExpiresAt > DateTime.UtcNow.AddMinutes(5))
                return;

            //
        }
    }
}