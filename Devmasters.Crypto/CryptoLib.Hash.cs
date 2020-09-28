using System;
using System.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
/* Unmerged change from project 'Devmasters.Crypto (net472)'
Before:
using System.Security.Cryptography;
After:
using System.Text;
*/


namespace Devmasters.Crypto
{
    public class Hash
    {


        public static byte[] ComputeHash(byte[] data)
        {
            HashAlgorithm algo;
            algo = MD5.Create();
            byte[] hash = algo.ComputeHash(data);
            return data;
        }

        public static byte[] ComputeHash(string text)
        {
            return ComputeHash(Encoding.UTF8.GetBytes(text));
        }


        public static string ComputeHashToBase64(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static string ComputeHashToBase64(string s)
        {

            // Pøevést vstupní øetìzec na pole bajtù
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);

            // Spoèítat MD5 hash
            string r;
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] hash = md5.ComputeHash(data);

                // Pøevést na Base64
                r = Convert.ToBase64String(hash);
            }

            return r;
        }


        public static string ComputeHashToHex(string s)
        {

            // Pøevést vstupní øetìzec na pole bajtù
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);

            // Spoèítat MD5 hash
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] hash = md5.ComputeHash(data);
                // Build the final string by converting each byte
                // into hex and appending it to a StringBuilder
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }

                // And return it
                return sb.ToString();
            }

        }


    }
}
