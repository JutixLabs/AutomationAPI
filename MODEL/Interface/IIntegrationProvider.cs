using AutomationAPI.MODEL.Entity;

namespace AutomationAPI.MODEL.Interface
{
    public interface IIntegrationProvider
    {
        string Provider { get; }
        Task ExecuteAsync(ConnectedApp app, string action, Dictionary<string,object> payload);
    }
}
