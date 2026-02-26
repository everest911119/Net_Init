using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public static class HashHelper
    {
        private static string ToHashString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();    
            for (int i = 0;i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public static string ComputeSha256Hash(Stream stream)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(stream);
                return ToHashString(bytes);
            }
        }

        public static string ComputeSha256Hash(string input)
        {
            using (SHA256 sHA256 = SHA256.Create())
            {
                byte[] bytes = sHA256.ComputeHash(Encoding.UTF8.GetBytes(input));   
                return ToHashString(bytes);
            }
        }
    }
}
