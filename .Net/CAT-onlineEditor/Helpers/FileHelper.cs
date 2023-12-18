using System;
using System.IO;

namespace CAT.Helpers
{
    public class FileHelper
    {
        public string GetUniqueFileName(string originalFileName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            string fileExtension = Path.GetExtension(originalFileName);
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            // Combine the original file name without extension, timestamp, and file extension to create a unique file name
            string uniqueFileName = $"{fileNameWithoutExtension}_{timeStamp}{fileExtension}";

            return uniqueFileName;
        }
    }
}
