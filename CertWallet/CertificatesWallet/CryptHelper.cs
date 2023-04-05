using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CertificatesWallet
{
    public static class CryptHelper
    {
        public static string GenerateAESKey(int keySize = 256)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = keySize;
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }
        public static byte[] EncryptAES(byte[] data, string key)
        {
            byte[] keyBytes = Convert.FromBase64String(CleanKeyString(key));
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = keyBytes.Length * 8;
                aes.IV = new byte[aes.IV.Length];
                aes.Key = keyBytes;

                return aes.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
            }
        }
        public static byte[] DecryptAES(byte[] data, string key)
        {
            byte[] keyBytes = Convert.FromBase64String(CleanKeyString(key));
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = keyBytes.Length * 8;
                aes.IV = new byte[aes.IV.Length];
                aes.Key = keyBytes;

                return aes.CreateDecryptor().TransformFinalBlock(data, 0, data.Length);
            }
        }


        public static string CleanKeyString(string value)
        {
            value = Uri.UnescapeDataString(value);
            int chars = 4 - value.Length % 4;
            if (chars == 4 || chars == 0)
                return value;
            return value + new string('=', value.Length % 4);
        }
    }
}
