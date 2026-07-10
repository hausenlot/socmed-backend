namespace socmed_backend.Services;

public interface IEncryptionService
{
    (string cipherText, string iv) Encrypt(string plainText);
    string Decrypt(string cipherText, string iv);
}
