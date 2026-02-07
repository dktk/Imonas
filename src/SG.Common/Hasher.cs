using System.Security.Cryptography;
using System.Text;

namespace System
{
    public class Hasher
    {
        public static int Hash(string value)
        {
            var md5Hasher = MD5.Create();

            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(value));

            return Math.Abs(BitConverter.ToInt32(hashed, 0));
        }
        
        public static string HashString(string value)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(value, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
