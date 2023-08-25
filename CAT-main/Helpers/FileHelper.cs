using System;
using System.IO;
using System.Security.Cryptography;

namespace CAT.Helpers
{
    public class FileHelper
    {
        public static string GetUniqueFileName(string originalFileName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            string fileExtension = Path.GetExtension(originalFileName);
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            // Combine the original file name without extension, timestamp, and file extension to create a unique file name
            string uniqueFileName = $"{fileNameWithoutExtension}_{timeStamp}{fileExtension}";

            return uniqueFileName;
        }

        public static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
