using AutomationAPI.MODEL.Interface;
using System.Security.Cryptography;

namespace AutomationAPI.DATA.Congiguration
{
    public class CustomIdGenerator : ICustomIdGenerator
    {
        public string RandomDigits(int digits)
        {
            byte[] buffer = new byte[8];
            RandomNumberGenerator.Fill(buffer);

            long number = Math.Abs(BitConverter.ToInt64(buffer, 0));
            long mod = (long)Math.Pow(10, digits);
            number %= mod;

            return number.ToString($"D{digits}");
        }

        public string TimeStamped(string prefix, int digits, string format = "yyyy-MM")
        {
            string TimeStamp = DateTime.UtcNow.ToString(format);
            string randomDigits = RandomDigits(digits);

            return $"{prefix}-{TimeStamp}-{randomDigits}";
        }
    }
}
