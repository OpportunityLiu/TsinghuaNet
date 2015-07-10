using Windows.Security.Cryptography.Core;
using static Windows.Security.Cryptography.CryptographicBuffer;
using Windows.Security.Cryptography;

namespace TsinghuaNet
{
    internal static class MD5Helper 
    {
        private static HashAlgorithmProvider md5 = HashAlgorithmProvider.OpenAlgorithm("MD5");

        public static string GetMd5Hash(string input)
        {
            return EncodeToHexString(md5.HashData(ConvertStringToBinary(input, BinaryStringEncoding.Utf8)));
        }
    }
}
