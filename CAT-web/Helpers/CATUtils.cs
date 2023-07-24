using BU;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace CAT_web.Helpers
{
    public class CATUtils
    {
        /// <summary>
        /// IsCompressedMemoQXliff
        /// </summary>
        /// <param name="sFilePath"></param>
        /// <returns></returns>
        public static bool IsCompressedMemoQXliff(String sFilePath)
        {
            String sExt = Path.GetExtension(sFilePath).ToLower();
            if (sExt == ".mqxlz" || sExt == ".xlz")
                return true;

            return false;
        }

        public static String ExtractMQXlz(String sFilePath)
        {
            String sOutDir = ConfigurationSettings.AppSettings["TempFolder"];
            String sNewXlfPath = Path.Combine(sOutDir, Guid.NewGuid().ToString() + ".xlf");
            try
            {
                String sExt = Path.GetExtension(sFilePath).ToLower();
                //unzip the file if it is zipped
                if (sExt == ".xlz" || sExt == ".mqxlz")
                {
                    String sTmpDir = Path.Combine(sOutDir, Guid.NewGuid().ToString());
                    ZipHelper.UnZipFiles(sFilePath, sTmpDir, null);
                    //copy the file into the out dir
                    if (sExt == ".xlz")
                    {
                        String[] aFiles = Directory.GetFiles(sTmpDir, "*.xlf");
                        File.Copy(aFiles[0], sNewXlfPath);
                    }
                    else
                    {
                        //mqxlz
                        File.Copy(sTmpDir + "/document.mqxliff", sNewXlfPath);
                    }

                    //delete the zip folder
                    Directory.Delete(sTmpDir, true);
                }
                else
                    File.Copy(sFilePath, sNewXlfPath);

                return sNewXlfPath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
