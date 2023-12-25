using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace CAT.Helpers
{
    /// <summary>
    /// Summary description for cZipHelper.
    /// </summary>
    public class ZipHelper
    {
        public ZipHelper()
        {
        }

        public bool ZipFiles(string[] FilenamesToPack, string sZipName, bool bKeepDirectoryStructure)
        {
            return ZipFiles(FilenamesToPack, null, sZipName, bKeepDirectoryStructure);
        }
        public bool ZipFiles(string[] FilenamesToPack, System.Collections.Specialized.StringCollection? asNamesInThePack, string sZipName,
            bool bKeepDirectoryStructure)
        {
            bool bRet = true;
            var crc = new Crc32();
            var s = new ZipOutputStream(File.Create(sZipName));

            s.SetLevel(6); // 0 - store only to 9 - means best compression
            int i = -1;

            foreach (string file in FilenamesToPack)
            {
                i++;
                FileStream fs = default!;
                try
                {
                    fs = File.OpenRead(file);
                }
                catch
                {
                    bRet = false;
                    continue;
                }
                var sNameInPack = asNamesInThePack == null ? file : asNamesInThePack[i]!;
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                ZipEntry entry;
                if (bKeepDirectoryStructure)
                    entry = new ZipEntry(sNameInPack);
                else
                    entry = new ZipEntry(Path.GetFileName(sNameInPack));

                entry.DateTime = DateTime.Now;

                // set Size and the crc, because the information
                // about the size and crc should be stored in the header
                // if it is not set it is automatically written in the footer.
                // (in this case size == crc == -1 in the header)
                // Some ZIP programs have problems with zip files that don't store
                // the size and crc in the header.
                entry.Size = fs.Length;
                fs.Close();

                crc.Reset();
                crc.Update(buffer);

                entry.Crc = crc.Value;

                s.PutNextEntry(entry);

                s.Write(buffer, 0, buffer.Length);

            }

            s.Finish();
            s.Close();
            return bRet;
        }

        public void ZipDirectory(string sDirectory, string sZipName)
        {
            string[] filenames = Directory.GetFiles(sDirectory);

            var crc = new Crc32();
            var s = new ZipOutputStream(File.Create(sZipName));

            s.SetLevel(6); // 0 - store only to 9 - means best compression

            foreach (string file in filenames)
            {
                FileStream fs = File.OpenRead(file);

                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                var entry = new ZipEntry(file)
                {
                    DateTime = DateTime.Now,

                    // set Size and the crc, because the information
                    // about the size and crc should be stored in the header
                    // if it is not set it is automatically written in the footer.
                    // (in this case size == crc == -1 in the header)
                    // Some ZIP programs have problems with zip files that don't store
                    // the size and crc in the header.
                    Size = fs.Length
                };
                fs.Close();

                crc.Reset();
                crc.Update(buffer);

                entry.Crc = crc.Value;

                s.PutNextEntry(entry);

                s.Write(buffer, 0, buffer.Length);

            }

            s.Finish();
            s.Close();
        }

        public static void ZipDirectoryKeepRelativeSubfolder(String directoryToZip, String zipFilePath)
        {
            var filenames = Directory.GetFiles(directoryToZip, "*.*", SearchOption.AllDirectories);
            using var s = new ZipOutputStream(File.Create(zipFilePath));
            s.SetLevel(9);// 0 - store only to 9 - means best compression

            var buffer = new byte[4096];

            foreach (var file in filenames)
            {
                var relativePath = file[directoryToZip.Length..].TrimStart('\\');
                var entry = new ZipEntry(relativePath) { DateTime = DateTime.Now };
                s.PutNextEntry(entry);

                using var fs = File.OpenRead(file);
                int sourceBytes = 0;
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    s.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
            s.Finish();
            s.Close();
        }

        public static void UnZipFiles(string zipPathAndFile, string outputFolder, string? password)
        {
            using var zipInputStream = new ZipInputStream(File.OpenRead(zipPathAndFile));
            if (!string.IsNullOrEmpty(password))
                zipInputStream.Password = password;

            ZipEntry entry;
            while ((entry = zipInputStream.GetNextEntry()) != null)
            {
                if (!string.IsNullOrEmpty(entry.Name) && !entry.Name.EndsWith(".ini"))
                {
                    string fullPath = Path.Combine(outputFolder, entry.Name);
                    string directoryPath = Path.GetDirectoryName(fullPath!)!;

                    if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    if (Path.GetFileName(fullPath) != string.Empty)
                    {
                        using FileStream fileStream = File.Create(fullPath);
                        zipInputStream.CopyTo(fileStream);
                    }
                }
            }
        }
    }
}
