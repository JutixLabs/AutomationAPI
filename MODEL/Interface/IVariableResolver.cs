namespace AutomationAPI.MODEL.Interface
{
    public interface IVariableResolver
    {
        string Resolve(string template, Dictionary<string, object> payload);
    }
}
