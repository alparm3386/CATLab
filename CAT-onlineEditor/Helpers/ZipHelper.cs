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
			//string[] filenames = Directory.GetFiles(args[0]);

			bool bRet = true;
			Crc32 crc = new Crc32();
			ZipOutputStream s = new ZipOutputStream(File.Create(sZipName));
		
			s.SetLevel(6); // 0 - store only to 9 - means best compression
			int i = -1;
			string sNameInPack;
		
			foreach (string file in FilenamesToPack) 
			{
				i++;
				FileStream fs;
				try
				{
					fs= File.OpenRead(file);
				}
				catch
				{
					//return false;
					bRet = false;
					continue;
				}
				sNameInPack = asNamesInThePack == null ? file : asNamesInThePack[i];
				byte[] buffer = new byte[fs.Length];
				fs.Read(buffer, 0, buffer.Length);
                ZipEntry entry = null;
                if(bKeepDirectoryStructure)
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
			
				entry.Crc  = crc.Value;
			
				s.PutNextEntry(entry);
			
				s.Write(buffer, 0, buffer.Length);
			
			}
		
			s.Finish();
			s.Close();
			return bRet;
		}
		
		public void ZipDirectory(string sDirectory,string sZipName)
		{
			string[] filenames = Directory.GetFiles(sDirectory);
		
			Crc32 crc = new Crc32();
			ZipOutputStream s = new ZipOutputStream(File.Create(sZipName));
		
			s.SetLevel(6); // 0 - store only to 9 - means best compression
		
			foreach (string file in filenames) 
			{
				FileStream fs = File.OpenRead(file);
			
				byte[] buffer = new byte[fs.Length];
				fs.Read(buffer, 0, buffer.Length);
				ZipEntry entry = new ZipEntry(file);
			
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
			
				entry.Crc  = crc.Value;
			
				s.PutNextEntry(entry);
			
				s.Write(buffer, 0, buffer.Length);
			
			}
		
			s.Finish();
			s.Close();
		}

        public static void ZipDirectoryKeepRelativeSubfolder(String directoryToZip, String zipFilePath)
        {
            var filenames = Directory.GetFiles(directoryToZip, "*.*", SearchOption.AllDirectories);
            using (var s = new ZipOutputStream(File.Create(zipFilePath)))
            {
                s.SetLevel(9);// 0 - store only to 9 - means best compression

                var buffer = new byte[4096];

                foreach (var file in filenames)
                {
                    var relativePath = file.Substring(directoryToZip.Length).TrimStart('\\');
                    var entry = new ZipEntry(relativePath);
                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);

                    using (var fs = File.OpenRead(file))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }
                s.Finish();
                s.Close();
            }
        }

        public static void UnZipFiles(string zipPathAndFile, string outputFolder, string? password/*, bool deleteZipFile*/)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
            if (password != null && password != String.Empty)
                s.Password = password;
            ZipEntry theEntry;
            string tmpEntry = String.Empty;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = outputFolder;
                string fileName = Path.GetFileName(theEntry.Name);
                // create directory 
                if (directoryName != "")
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (fileName != String.Empty)
                {
                    if (theEntry.Name.IndexOf(".ini") < 0)
                    {
                        string fullPath = directoryName + "\\" + theEntry.Name;
                        fullPath = fullPath.Replace("\\ ", "\\");
                        string fullDirPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                        FileStream streamWriter = File.Create(fullPath);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        streamWriter.Close();
                    }
                }
            }
            s.Close();
//            if (deleteZipFile)
//                File.Delete(zipPathAndFile);
        }
    }
}
