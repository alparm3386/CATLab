using System;
using System.IO;
using System.Security.Cryptography;

namespace CAT.Helpers
{
    public class FileHelper
    {
        public static string GetUniqueFileName(string filePath)
        {
            if (!File.Exists(filePath))
                return Path.GetFileName(filePath);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            // Combine the original file name without extension, timestamp, and file extension to create a unique file name
            string uniqueFileName = $"{fileNameWithoutExtension}_{timeStamp}{fileExtension}";

            return uniqueFileName;
        }

        public static string GetUniqueFileName(string originalFileName, string md5)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            string fileExtension = Path.GetExtension(originalFileName);
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            // Combine the original file name without extension, timestamp, and file extension to create a unique file name
            string uniqueFileName = $"{fileNameWithoutExtension}_{timeStamp}{fileExtension}";

            return uniqueFileName;
        }

        public static string CalculateMD5(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return CalculateMD5(fileStream);
            }
        }

        public static string CalculateMD5(Stream fileStream)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(fileStream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
