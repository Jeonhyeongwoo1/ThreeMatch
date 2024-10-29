using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class AesEncryptionHelper
{
    private static byte[] _key;
    private static byte[] _iv;

    public static void Initialize(string key, string iv)
    {
        _key = Encoding.UTF8.GetBytes(key);
        _iv = Encoding.UTF8.GetBytes(iv);
    }

    public static string Encrypt(string plainText)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = _key;
        aesAlg.IV = _iv;
        
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new MemoryStream();
        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = _key;
        aesAlg.IV = _iv;
        
        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
        {
            return srDecrypt.ReadToEnd();
        }
    }
}