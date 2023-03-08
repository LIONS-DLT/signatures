using System.Numerics;
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

        public static BigInteger HashToBigInteger(string hash)
        {
            hash = hash.Replace('-', '+').Replace('_', '/');
            int n = hash.Length % 4;
            if (n > 0)
                hash += new string('=', 4 - n);

            byte[] data = Convert.FromBase64String(hash);
            
            return new BigInteger(data, true); // true -> unsigned!
        }
        public static string BigIntegerToHash(BigInteger value)
        {
            byte[] hash = value.ToByteArray();
            return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').Replace("=", "");
        }
    }

    public enum CryptAlgorithm
    {
        RSA
    }
}
