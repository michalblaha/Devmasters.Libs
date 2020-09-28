using System;



namespace Devmasters
{
    public partial class Core
    {
        public static System.Random Rnd = new Random();


        /// <summary>
        /// returns ASCII value of char in ISO-8859-1 charset. 
        /// C# implementation of Asc function
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static byte Asc(char src)
        {
            return (System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(src + "")[0]);
        }

        /// <summary>
        /// returns char with ASCII value src in ISO-8859-1 charset. 
        /// C# implementation of Chr function
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static char Chr(byte src)
        {
            return (System.Text.Encoding.GetEncoding("iso-8859-1").GetChars(new byte[] { src })[0]);
        }

        /// <summary>
        /// Returns the left part of a character string with the specified number of characters
        /// C# implementation of Left function
        /// </summary>
        /// <param name="host"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string Left(string host, int index)
        {
            if (index > host.Length)
                return host;
            return host.Substring(0, index);
        }


        /// <summary>
        /// Returns the right part of a character string with the specified number of characters
        /// C# implementation of Right function
        /// </summary>
        /// <param name="host"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string Right(string host, int index)
        {
            if (index > host.Length)
                return string.Empty;
            return host.Substring(host.Length - index);
        }


        public static bool IsInRange<T>(T value, T minValue, T maxValue) where T : IComparable<T>
        {
            if (minValue.CompareTo(maxValue) > 0)
                return (value.CompareTo(maxValue) >= 0 && value.CompareTo(minValue) <= 0);
            else
                return (value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0);
        }
    }


}
