using System;
using System.Security.Cryptography;
using System.Text;

namespace CastleHero.Common.Secure
{
    public class AES128
    {
        private string key {
            get {
                MD5 md5 = MD5.Create();
 
                string salt = "SoguemSogeumkkzz";
                byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(salt));
                return Encoding.UTF8.GetString(result);
            }
        }
 
        RijndaelManaged rijndaelCipher;
 
        public AES128()
        {
            rijndaelCipher = new RijndaelManaged();
 
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 128;
            rijndaelCipher.BlockSize = 128;
        }
 
        public string Encrypt(string textToEncrypt)
        {
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[16];
 
            int len = pwdBytes.Length;
            if (len > keyBytes.Length) len = keyBytes.Length;
 
            Array.Copy(pwdBytes, keyBytes, len);
 
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
 
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
 
            byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);
 
            return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }
    
        public string Decrypt(string textToDecrypt)
        {
            byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[16];
 
            int len = pwdBytes.Length;
            if (len > keyBytes.Length) len = keyBytes.Length;
 
            Array.Copy(pwdBytes, keyBytes, len);
 
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
 
            byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
 
            return Encoding.UTF8.GetString(plainText);
        }
    }
}