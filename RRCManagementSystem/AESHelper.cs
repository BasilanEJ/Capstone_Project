using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RRCManagementSystem.Helpers
{
    public static class AESHelper
    {
        // =========================================
        // Load AES Key from Web.config (Base64 encoded)
        // =========================================
        private static readonly byte[] Key = Convert.FromBase64String(ConfigurationManager.AppSettings["AESKey"]);

        // Optional SHA256 Pepper for hashing
        private static readonly string Pepper = ConfigurationManager.AppSettings["SHA256Pepper"];

        // =========================================
        // Original AES-256 Methods (Legacy Support)
        // =========================================

        // Encrypt raw byte array
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
                    // Write IV at the start
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray(); // [IV + EncryptedData]
                }
            }
        }

        // Decrypt raw byte array
        public static byte[] Decrypt(byte[] encryptedData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Extract IV (first 16 bytes)
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

        // Wrapper methods for easy usage
        public static byte[] EncryptBytes(byte[] data) => Encrypt(data);
        public static byte[] DecryptBytes(byte[] encryptedData) => Decrypt(encryptedData);

        // Encrypt text to Base64 string
        public static string EncryptText(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = EncryptBytes(plainBytes);
            return Convert.ToBase64String(encryptedBytes);
        }

        // Decrypt Base64 string to plain text
        public static string DecryptText(string encryptedBase64Text)
        {
            if (string.IsNullOrEmpty(encryptedBase64Text))
                return null;

            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64Text);
            byte[] decryptedBytes = DecryptBytes(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        // =========================================
        // AES-256 Methods for Email Column Only
        // =========================================

        /// <summary>
        /// Encrypt email using AES-256 with random IV.
        /// Output format: Base64([IV + CipherText])
        /// </summary>
        public static string EncryptEmail(string plainEmail)
        {
            if (string.IsNullOrEmpty(plainEmail))
                return null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV(); // Unique IV per encryption

                using (var ms = new MemoryStream())
                {
                    // Write IV first
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainEmail);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    // Convert to Base64 for storage
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypt AES-256 encrypted email from Base64 back to plain text.
        /// </summary>
        public static string DecryptEmail(string encryptedBase64Email)
        {
            if (string.IsNullOrEmpty(encryptedBase64Email))
                return null;

            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64Email);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Extract IV (first 16 bytes)
                byte[] iv = new byte[16];
                Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length);
                        cs.FlushFinalBlock();
                    }

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        // =========================================
        // NEW AES-256 for Other Inquiry Fields
        // =========================================

        /// <summary>
        /// Generic encryption for any field (Street, Barangay, City, etc.)
        /// </summary>
        public static string EncryptField(string plainValue)
        {
            if (string.IsNullOrEmpty(plainValue))
                return null;

            return EncryptText(plainValue);
        }

        /// <summary>
        /// Generic decryption for any field.
        /// </summary>
        public static string DecryptField(string encryptedValue)
        {
            if (string.IsNullOrEmpty(encryptedValue))
                return null;

            return DecryptText(encryptedValue);
        }

        /// <summary>
        /// Encrypt all sensitive fields at once before inserting into InquirySimple.
        /// </summary>
        public static void EncryptForInquiry(
            ref string email, ref string contact, ref string street,
            ref string barangay, ref string city, ref string region,
            ref string country, ref string landmark)
        {
            email = EncryptEmail(email);
            contact = EncryptField(contact);
            street = EncryptField(street);
            barangay = EncryptField(barangay);
            city = EncryptField(city);
            region = EncryptField(region);
            country = EncryptField(country);
            landmark = EncryptField(landmark);
        }

        // =========================================
        // SHA-256 for EmailHash Column
        // =========================================

        /// <summary>
        /// Compute SHA-256 hash of plain email (lowercased).
        /// Used for uniqueness and indexing in database.
        /// </summary>
        public static string ComputeSHA256(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            using (var sha = SHA256.Create())
            {
                // Always lowercasing to ensure consistent hashing
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input.ToLowerInvariant()));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Compute SHA-256 hash with a pepper value for extra security.
        /// </summary>
        public static string ComputeSHA256WithPepper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            if (string.IsNullOrEmpty(Pepper))
                return ComputeSHA256(input); // fallback if no pepper

            using (var sha = SHA256.Create())
            {
                string saltedInput = input.ToLowerInvariant() + Pepper;
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(saltedInput));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        // =========================================
        // Safe Utilities
        // =========================================

        /// <summary>
        /// Safely decode Base64 strings, return null if invalid.
        /// </summary>
        public static byte[] SafeBase64Decode(string base64)
        {
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }
    }
}
