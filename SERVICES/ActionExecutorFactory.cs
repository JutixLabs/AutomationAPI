using AutomationAPI.MODEL.Interface;

namespace AutomationAPI.SERVICES
{
    public class ActionExecutorFactory : IActionExecutorFactory
    {
        private readonly IEnumerable<IActionExecutor> _executor;
        public ActionExecutorFactory(IEnumerable<IActionExecutor> executor)
        {
            _executor = executor;
        }
        public IActionExecutor GetExecutor(string actionKey)
        {
            var provider = actionKey.Split('.')[0].ToLower();

            var executor = _executor.FirstOrDefault(e => e.Provider == provider);

            if (executor == null)
                throw new Exception($"No executor found for {provider}.");

            return executor;
        }
    }
}
