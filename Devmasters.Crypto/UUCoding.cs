using System;

namespace Devmasters.Crypto
{
    public static class UUCoding
    {

        public static string UUEncode(System.IO.Stream inFile)
        {
            byte[] buffer = new byte[inFile.Length];
            int c = inFile.Read(buffer, 0, (int)inFile.Length);
            return UUEncode(buffer);
        }

        public static string UUEncode(byte[] inData)
        {
            return Convert.ToBase64String(inData, Base64FormattingOptions.InsertLineBreaks);
        }

        public static byte[] UUDecode(string encodedData)
        {
            return Convert.FromBase64String(encodedData);
        }

    }
}
