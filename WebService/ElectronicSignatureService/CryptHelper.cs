using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicSignatureService
{
    public static class CryptHelper
    {
        public static string ToSHA256(this string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public static string ToSHA256_Base64(this string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').Replace("=", "");
            }
        }
    }

    public enum CryptAlgorithm
    {
        RSA
    }
}
