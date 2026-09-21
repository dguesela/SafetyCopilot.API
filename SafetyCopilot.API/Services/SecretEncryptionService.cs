using global::SafetyCopilot.API.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;


namespace SafetyCopilot.API.Services
{
    
    public class SecretEncryptionService
        : ISecretEncryptionService
    {
        private readonly IDataProtector _protector;

        public SecretEncryptionService(
            IDataProtectionProvider provider)
        {
            _protector =
                provider.CreateProtector(
                    "SafetyCopilot.OpenAiApiKey.v1");
        }

        public string Encrypt(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Value cannot be empty.",
                    nameof(value));
            }

            return _protector.Protect(
                value.Trim());
        }

        public string Decrypt(
            string encryptedValue)
        {
            if (string.IsNullOrWhiteSpace(
                    encryptedValue))
            {
                throw new ArgumentException(
                    "Encrypted value cannot be empty.",
                    nameof(encryptedValue));
            }

            return _protector.Unprotect(
                encryptedValue);
        }
    }
}
