using System;
using System.Security.Cryptography;
using UnityEngine;

namespace RGLabs.Common.Secure
{
    public static class EncryptStore
    {
        private const int KeySize = 16;
        private const int IvSize = 16;

        public static void SetString(string key, string value)
        {
            var encryptionKey = LoadOrGenerateKey("key", KeySize);
            var iv = LoadOrGenerateKey("iv", IvSize);

            var k = EncryptString(key, encryptionKey, iv);
            var v = EncryptString(value, encryptionKey, iv);

#if UNITY_EDITOR
            Debug.Log($"[EncryptStore.Set] {key} => {k} (key), {value} => {v} (value).");
#endif

            PlayerPrefs.SetString(k, v);
        }

        public static string GetString(string key)
        {
            var encryptionKey = LoadOrGenerateKey("key", KeySize);
            var iv = LoadOrGenerateKey("iv", IvSize);
            var k = EncryptString(key, encryptionKey, iv);
            var value = PlayerPrefs.GetString(k, string.Empty);
            var v = DecryptString(value, encryptionKey, iv);

#if UNITY_EDITOR
            Debug.Log($"[EncryptStore.Get] {key} => {k} (key), {value} => {v} (value).");
#endif

            return string.IsNullOrEmpty(value)
                ? string.Empty
                : v;
        }

        private static string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var memoryStream = new System.IO.MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using (var streamWriter = new System.IO.StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        private static string DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new System.IO.MemoryStream(Convert.FromBase64String(cipherText));
            using var cryptoStream = new CryptoStream(memoryStream, decrypt, CryptoStreamMode.Read);
            using var streamReader = new System.IO.StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }

        private static byte[] LoadOrGenerateKey(string keyName, int size)
        {
            if (PlayerPrefs.HasKey(keyName))
            {
                var base64Key = PlayerPrefs.GetString(keyName);
                return Convert.FromBase64String(base64Key);
            }

            var key = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);

            PlayerPrefs.SetString(keyName, Convert.ToBase64String(key));
            PlayerPrefs.Save();

            return key;
        }
    }
}