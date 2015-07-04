using Windows.Security.Cryptography.Core;
using Converter = Windows.Security.Cryptography.CryptographicBuffer;
using Windows.Security.Cryptography;

namespace TsinghuaNet
{
    internal static class MD5Helper
    {
        private static HashAlgorithmProvider md5 = HashAlgorithmProvider.OpenAlgorithm("MD5");

        public static string GetMd5Hash(string input)
        {
            return Converter.EncodeToHexString(md5.HashData(Converter.ConvertStringToBinary(input, BinaryStringEncoding.Utf8)));
        }
    }
}
