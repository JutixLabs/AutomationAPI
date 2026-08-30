using AutomationAPI.DATA;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.SERVICES
{
    public class IntegrationCredentialService : IIntegrationCredentialService
    {
        private readonly AppDbContext _dbContext;
        private readonly ISecretProtector _secretProtector;
        public IntegrationCredentialService(AppDbContext dbContext, ISecretProtector secretProtector)
        {
            _dbContext = dbContext;
            _secretProtector = secretProtector;
        }

        public Task<bool> ExistsAsync(string provider)
        {
            return _dbContext.IntegrationCredentials.AnyAsync(x => x.Provider == provider && x.IsActive);
        }

        public async Task<string> GetCredentialAsync(string provider)
        {
            var credentials = await _dbContext.IntegrationCredentials
                .FirstOrDefaultAsync(x => x.Provider == provider && x.IsActive);

            if (credentials == null)
                throw new Exception($"{provider} credentials not configured.");

            return _secretProtector.Unprotect(credentials.Value);
        }

        public async Task SaveCredentialAsync(IntegrationCredential credential)
        {
            // Upsert rather than pure-insert — calling this again for the same provider
            // (e.g. rotating the Discord bot token) should replace the active credential,
            // not leave two active rows for GetCredentialAsync to pick between arbitrarily.
            var existing = await _dbContext.IntegrationCredentials
                .FirstOrDefaultAsync(x => x.Provider == credential.Provider && x.IsActive);

            credential.Value = _secretProtector.Protect(credential.Value);

            if (existing != null)
            {
                existing.Value = credential.Value;
                existing.Name = credential.Name;
                existing.CredentialType = credential.CredentialType;
            }
            else
            {
                credential.CreatedAt = DateTime.UtcNow;
                await _dbContext.IntegrationCredentials.AddAsync(credential);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
