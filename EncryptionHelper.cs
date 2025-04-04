using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionHelper
{
    // In a real-world application, do not hard-code your key.
    // Use a secure way to store and retrieve your encryption key.
    //private static readonly string EncryptionKey = "Your32CharLongEncryptionKey!"; // 32 chars for AES-256
    private static readonly string EncryptionKey = "12345678901234567890123456789012"; // 32 chars for AES-256


    public static string EncryptString(string plainText)
    {
        byte[] key = Encoding.UTF8.GetBytes(EncryptionKey);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.GenerateIV(); // Generates a new IV for each encryption.
            byte[] iv = aesAlg.IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);
            using (var msEncrypt = new MemoryStream())
            {
                // Prepend the IV to the encrypted data.
                msEncrypt.Write(iv, 0, iv.Length);
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    public static string DecryptString(string cipherText)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherText);
        byte[] iv = new byte[16]; // AES block size is 16 bytes.
        Array.Copy(fullCipher, iv, iv.Length);
        byte[] cipher = new byte[fullCipher.Length - iv.Length];
        Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        byte[] key = Encoding.UTF8.GetBytes(EncryptionKey);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using (var msDecrypt = new MemoryStream(cipher))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }
}
