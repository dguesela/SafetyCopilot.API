namespace SafetyCopilot.API.Services.Interfaces
{
    public interface ISecretEncryptionService
    {
        string Encrypt(string value);

        string Decrypt(string encryptedValue);
    }
}
