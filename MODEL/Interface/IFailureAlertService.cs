namespace AutomationAPI.MODEL.Interface
{
    public interface IFailureAlertService
    {
        Task SendFailureAlertAsync(string userId, string message);
    }
}
