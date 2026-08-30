namespace AutomationAPI.MODEL.Interface
{
    public interface IResourceProviderResolver
    {
        IResourceProvider Resolve(string provider); 
    }
}
