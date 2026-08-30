namespace AutomationAPI.MODEL.Interface
{
    public interface ICustomIdGenerator
    {
        string RandomDigits(int digits);
        string TimeStamped(string prefix, int digits, string format = "yyyy-MM");
    }
}
