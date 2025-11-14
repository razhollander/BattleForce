using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CoreDomain.Scripts.Utils
{
    public static class EncryptionUtils
    {
        private static readonly string Key = "X9r2TmA6qWz4LpYvB83cM1nGJ0tUeKfZ";

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.GenerateIV();
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, iv);
            using var ms = new MemoryStream();
            ms.Write(iv, 0, iv.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string encrypted)
        {
            var fullData = Convert.FromBase64String(encrypted);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);

            var iv = new byte[16];
            Array.Copy(fullData, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(fullData, 16, fullData.Length - 16);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}