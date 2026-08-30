namespace AutomationAPI.MODEL.Interface
{
    public interface ISecretProtector
    {
        string Protect(string value);
        string Unprotect(string value);
    }
}
