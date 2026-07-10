using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace socmed_backend.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        // Use a configured key, fallback to a development default if not specified
        var keyString = configuration["Encryption:Key"] ?? "a-default-fallback-encryption-key-for-development-32bytes";
        
        // Ensure the key is exactly 256 bits (32 bytes) by hashing it
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
    }

    public (string cipherText, string iv) Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) 
            return (string.Empty, string.Empty);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        var iv = Convert.ToBase64String(aes.IV);

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        var cipherText = Convert.ToBase64String(ms.ToArray());
        return (cipherText, iv);
    }

    public string Decrypt(string cipherText, string iv)
    {
        if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(iv)) 
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = Convert.FromBase64String(iv);

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            
            return sr.ReadToEnd();
        }
        catch
        {
            return "[Decryption Failed]";
        }
    }
}
