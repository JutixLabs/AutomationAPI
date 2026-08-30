using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IConnectAppsService
    {
        Task<List<ConnectedApp>> GetConnectedAppsAsync();
        Task<ConnectedApp> ConnectAppAsync(ConnectAppDTO model);
        Task<string> DeactivateAppAsync(int id);



        Task<ConnectedApp> GetConnectionAsync(string provider);
        Task<string> GetAccessTokenAsync(string provider);
        Task RefreshTokenIfNeededAsync(string provider);
    }
}
