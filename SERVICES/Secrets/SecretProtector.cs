using AutomationAPI.MODEL.Interface;
using Microsoft.AspNetCore.DataProtection;

namespace AutomationAPI.SERVICES.Secrets
{
    public class SecretProtector : ISecretProtector
    {
        private readonly IDataProtector _protector;
        public SecretProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("Jutix.Automation.Secrets");
        }
        public string Protect(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            return _protector.Protect(value);
        }

        public string Unprotect(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            return _protector.Unprotect(value);
        }
    }
}
