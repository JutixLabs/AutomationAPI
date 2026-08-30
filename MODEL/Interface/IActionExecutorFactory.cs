namespace AutomationAPI.MODEL.Interface
{
    public interface IActionExecutorFactory
    {
        IActionExecutor GetExecutor(string actionKey);
    }
}
