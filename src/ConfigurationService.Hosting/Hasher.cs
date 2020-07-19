using System.Security.Cryptography;
using System.Text;

namespace ConfigurationService.Hosting
{
    public static class Hasher
    {
        public static string CreateHash(byte[] bytes)
        {
            using (var hash = SHA1.Create())
            {
                var hashBytes = hash.ComputeHash(bytes);

                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}