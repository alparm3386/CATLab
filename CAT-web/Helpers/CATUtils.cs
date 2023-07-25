using BU;
using CAT_web.Enums;
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
            String sOutDir = System.Configuration.ConfigurationManager.AppSettings["TempFolder"]!;
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

        public static String GetJobDataFolder(int idJob)
        {
            var jobDataBaseFolder = System.Configuration.ConfigurationManager.AppSettings["jobDataBaseFolder"]!;

            var jobDataFolder = Path.Combine(jobDataBaseFolder, idJob.ToString());

            //create the folder if it doesn't exists
            if (!Directory.Exists(jobDataFolder))
                Directory.CreateDirectory(jobDataFolder);

            return jobDataFolder;
        }

        public static String CreateXlfFilePath(int idJob, DocumentType documentType)
        {
            var jobDataFolder = GetJobDataFolder(idJob);
            var xliffPath = Path.Combine(jobDataFolder, documentType.ToString() + ".xliff");
            if (File.Exists(xliffPath))
            {
                //do a backup
                File.Copy(xliffPath, Path.Combine(jobDataFolder, Path.GetFileNameWithoutExtension(xliffPath) + "_" +
                    DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xlf"));
            }

            return xliffPath; 
        }

        public static bool IsSegmentEmptyOrWhiteSpaceOnly(String sXml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            if (!sXml.StartsWith("<seg>"))
                sXml = "<seg>" + sXml + "</seg>";
            xmlDoc.LoadXml(sXml);
            XmlNodeList nodeList = xmlDoc.ChildNodes[0].ChildNodes;

            try
            {
                var sbTextContent = new StringBuilder();
                foreach (XmlNode node in nodeList)
                {
                    if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.Whitespace)
                        sbTextContent.Append(node.InnerText);
                }

                if (sbTextContent.Length == 0)
                    return true;

                var matces = Regex.Match(sbTextContent.ToString(), @"\A\s*\z");
                return matces.Length > 0;
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        public static String XliffTags2TMXTags(String sXliffContent)
        {
            String sConverted = sXliffContent;
            sConverted = Regex.Replace(sConverted, "<x id=['\"].*?['\"].*?/>", "<ph type='fmt'>{}</ph>"); //no x element in TMX 
            sConverted = Regex.Replace(sConverted, "(?<=<[^>]*?)(id)", "i"); //regex seems to be faster than xslt
            sConverted = Regex.Replace(sConverted, "(?<=<[^>]*?)(ctype)", "type");
            sConverted = Regex.Replace(sConverted, "(?<=<[^>]*?)(type=\"underlined\")", "type=\"ulined\"");
            sConverted = sConverted.Replace(" xmlns=\"urn:oasis:names:tc:xliff:document:1.2\"", ""); //is there better solution?

            return sConverted;
        }

        public enum TagType { Xliff, Tmx };

        public static String XmlTags2GoogleTags(String sXml, TagType tagType)
        {
            //collect the ids to skip
            StringBuilder sRet = new StringBuilder();
            MatchCollection matches = Regex.Matches(sXml, @"{\s*(?<id>\d+)\s*}|{\s*/\s*(?<id>\d+)\s*}|{\s*(?<id>\d+)\s*/\s*}");
            HashSet<String> idsToSkip = new HashSet<String>();
            foreach (Match match in matches)
                idsToSkip.Add(match.Groups["id"].Value);

            String sIdAttr = "id";
            if (tagType == TagType.Tmx)
                sIdAttr = "i";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            if (!sXml.StartsWith("<seg>"))
                sXml = "<seg>" + sXml + "</seg>";
            xmlDoc.LoadXml(sXml);
            XmlNodeList nodeList = xmlDoc.ChildNodes[0].ChildNodes;
            var openTags = new Dictionary<String, String>();
            int id = 1;
            String outerXml = "";
            try
            {
                foreach (XmlNode node in nodeList)
                {
                    outerXml = node.OuterXml;
                    while (idsToSkip.Contains(id.ToString()))
                        id++;
                    if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.Whitespace)
                        sRet.Append(node.InnerText);
                    else if (node.NodeType == XmlNodeType.Element)
                    {
                        switch (node.Name)
                        {
                            case "ph":
                            case "x":
                                sRet.Append("{" + id + "/}");
                                id++;
                                break;
                            case "bpt":
                                sRet.Append("{" + id + "}");
                                openTags.Add(node.Attributes[sIdAttr].Value, id.ToString());
                                id++;
                                break;
                            case "ept":
                                sRet.Append("{/" + openTags[node.Attributes[sIdAttr].Value] + "}");
                                break;
                            case "it":
                                sRet.Append("{" + id + "/}");
                                id++;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("id=" + id);
            }

            return sRet.ToString();
        }
    }
}
