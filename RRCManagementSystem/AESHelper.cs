using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RRCManagementSystem.Helpers
{
    public static class AESHelper
    {
        private static readonly byte[] Key = Convert.FromBase64String(ConfigurationManager.AppSettings["AESKey"]);

        // ✅ Original Encrypt method (for old pages)
        public static byte[] Encrypt(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV(); // Always generate a random IV

                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // Write IV at the start

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray(); // [IV + EncryptedData]
                }
            }
        }

        // ✅ Original Decrypt method (for old pages)
        public static byte[] Decrypt(byte[] encryptedData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] iv = new byte[16];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedData, iv.Length, encryptedData.Length - iv.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        // ✅ Wrapper for new usage in chat/files
        public static byte[] EncryptBytes(byte[] data)
        {
            return Encrypt(data);
        }

        public static byte[] DecryptBytes(byte[] encryptedData)
        {
            return Decrypt(encryptedData);
        }

        // ✅ BONUS: Text Encryption (string input/output)
        public static string EncryptText(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = EncryptBytes(plainBytes);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string DecryptText(string encryptedBase64Text)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64Text);
            byte[] decryptedBytes = DecryptBytes(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}