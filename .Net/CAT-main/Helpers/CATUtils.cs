﻿using CAT.Enums;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace CAT.Helpers
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

        public static String ExtractMQXlz(String sFilePath, String tempFolder)
        {
            String sNewXlfPath = Path.Combine(tempFolder, Guid.NewGuid().ToString() + ".xlf");
            try
            {
                String sExt = Path.GetExtension(sFilePath).ToLower();
                //unzip the file if it is zipped
                if (sExt == ".xlz" || sExt == ".mqxlz")
                {
                    String sTmpDir = Path.Combine(tempFolder, Guid.NewGuid().ToString());
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
            catch (Exception)
            {
                throw;
            }
        }

        public static String GetJobDataFolder(int jobId, String jobDataBaseFolder)
        {
            var jobDataFolder = Path.Combine(jobDataBaseFolder, jobId.ToString());

            //create the folder if it doesn't exists
            if (!Directory.Exists(jobDataFolder))
                Directory.CreateDirectory(jobDataFolder);

            return jobDataFolder;
        }

        public static String CreateXlfFilePath(int jobId, DocumentType documentType, String jobDataBaseFolder, bool withBackup)
        {
            var jobDataFolder = GetJobDataFolder(jobId, jobDataBaseFolder);
            var xliffPath = Path.Combine(jobDataFolder, documentType.ToString() + ".xliff");
            if (withBackup && File.Exists(xliffPath))
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
            XmlNodeList nodeList = xmlDoc!.ChildNodes[0]!.ChildNodes;

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
            catch (Exception)
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
            var sRet = new StringBuilder();
            MatchCollection matches = Regex.Matches(sXml, @"{\s*(?<id>\d+)\s*}|{\s*/\s*(?<id>\d+)\s*}|{\s*(?<id>\d+)\s*/\s*}");
            var idsToSkip = new HashSet<String>();
            foreach (Match match in matches)
                idsToSkip.Add(match.Groups["id"].Value);

            String sIdAttr = "id";
            if (tagType == TagType.Tmx)
                sIdAttr = "i";

            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            if (!sXml.StartsWith("<seg>"))
                sXml = "<seg>" + sXml + "</seg>";
            xmlDoc.LoadXml(sXml);
            XmlNodeList nodeList = xmlDoc!.ChildNodes![0]!.ChildNodes;
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
                                openTags.Add(node!.Attributes![sIdAttr]!.Value, id.ToString());
                                id++;
                                break;
                            case "ept":
                                sRet.Append("{/" + openTags[node!.Attributes![sIdAttr]!.Value] + "}");
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
            catch (Exception)
            {
                throw new Exception("id=" + id);
            }

            return sRet.ToString();
        }

        public static readonly int MARKER_OPENING = 0xE101;
        public static readonly int MARKER_CLOSING = 0xE102;
        public static readonly int MARKER_ISOLATED = 0xE103;
        public static readonly int CHARBASE = 0xE110;

        //regex for all TextFragment markers
        public static readonly String MARKERS_REGEX = "[\uE101\uE102\uE103\uE104].";

        public static string XliffSegmentToCodedText(String sXliffSegment)
        {
            try
            {
                if (sXliffSegment == null)
                    sXliffSegment = "";

                // remove the outer tag
                var sbOut = new StringBuilder();

                // matcher for the tmx tags
                var matches = Regex.Matches(sXliffSegment, "<[^>]*>[^>]*>");
                int id = 1;
                int prevEnd = 0;
                var idStack = new Stack<int>();
                String sText = "";
                foreach (Match match in matches)
                {
                    String sTag = match.Value.ToLower();
                    sText = sXliffSegment.Substring(prevEnd, match.Index - prevEnd);
                    sText = HttpUtility.HtmlDecode(sText); //xml decode
                    prevEnd = match.Index + match.Length;
                    sbOut.Append(sText);
                    if (sTag.StartsWith("<bpt"))
                    {
                        sbOut.Append("" + ((char)MARKER_OPENING) + (char)(CHARBASE + id));
                        idStack.Push(id);
                        id++;
                    }
                    else if (sTag.StartsWith("<ept"))
                    {
                        sbOut.Append("" + ((char)MARKER_CLOSING) + (char)(CHARBASE + idStack.Pop()));
                    }
                    else if (sTag.StartsWith("<ph") || sTag.StartsWith("<x"))
                    {
                        sbOut.Append("" + ((char)MARKER_ISOLATED) + (char)(CHARBASE + id));
                        id++;
                    }
                }

                sText = sXliffSegment.Substring(prevEnd, sXliffSegment.Length - prevEnd);
                sText = HttpUtility.HtmlDecode(sText); //xml decode
                sbOut.Append(sText);

                return sbOut.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("XliffSegmentToTextFragmentSimple: " + ex.Message);
            }
            finally
            {
            }
        }

        public static string CodedTextToXliff(string codedText)
        {
            //create simple codes
            var tmp = new StringBuilder();
            for (int i = 0; i < codedText.Length; i++)
            {
                var charCode = codedText[i];
                if (charCode == MARKER_OPENING)
                    tmp.Append("<bpt id=\"" + ((int)codedText[++i] - CHARBASE).ToString() + "\">{}</bpt>");
                else if (charCode == MARKER_CLOSING)
                    tmp.Append("<ept id=\"" + ((int)codedText[++i] - CHARBASE).ToString() + "\">{}</ept>");
                else if (charCode == MARKER_ISOLATED)
                    tmp.Append("<ph id=\"" + ((int)codedText[++i] - CHARBASE).ToString() + "\">{}</ph>");
                else
                {
                    //xml escape 
                    switch (charCode)
                    {
                        case '>': tmp.Append("&gt;"); break;
                        case '<': tmp.Append("&lt;"); break;
                        case '\r': tmp.Append("&#13;"); break; // Not a line-break in the XML context, but a literal
                        case '&': tmp.Append("&amp;"); break;
                        //case '"': tmp.Append("&quot;";
                        //case '\'': tmp.Append("&apos;"; //"&#39;"
                        default:
                            tmp.Append(charCode); break;
                    }
                }
            }

            return tmp.ToString();
        }

        public static String CodedTextToTmx(string codedText)
        {
            //create simple codes
            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < codedText.Length; i++)
            {
                var charCode = codedText[i];
                if (charCode == MARKER_OPENING)
                    tmp.Append("<bpt i=\"" + ((int)codedText[++i] - CHARBASE).ToString() + "\">{}</bpt>");
                else if (charCode == MARKER_CLOSING)
                    tmp.Append("<ept i=\"" + ((int)codedText[++i] - CHARBASE).ToString() + "\">{}</ept>");
                else if (charCode == MARKER_ISOLATED)
                    tmp.Append("<ph i=\"" + ((int)codedText[++i] - CHARBASE).ToString() + "\">{}</ph>");
                else
                {
                    //xml escape 
                    switch (charCode)
                    {
                        case '>': tmp.Append("&gt;"); break;
                        case '<': tmp.Append("&lt;"); break;
                        case '\r': tmp.Append("&#13;"); break; // Not a line-break in the XML context, but a literal
                        case '&': tmp.Append("&amp;"); break;
                        //case '"': tmp.Append("&quot;";
                        //case '\'': tmp.Append("&apos;"; //"&#39;"
                        default:
                            tmp.Append(charCode); break;
                    }
                }
            }

            return tmp.ToString();
        }

        public static string TmxSegmentToCodedText(String sTmxSeg)
        {
            try
            {
                if (String.IsNullOrEmpty(sTmxSeg))
                    return "";

                // remove the outer tag
                var sbOut = new StringBuilder();

                // matcher for the tmx tags
                var matches = Regex.Matches(sTmxSeg, "<[^>]*>[^>]*>");
                int id = 1;
                int prevEnd = 0;
                var idStack = new Stack<int>();
                String sText = "";
                foreach (Match match in matches)
                {
                    String sTag = match.Value.ToLower();
                    sText = sTmxSeg.Substring(prevEnd, match.Index - prevEnd);
                    sText = HttpUtility.HtmlDecode(sText); //xml decode
                    prevEnd = match.Index + match.Length;
                    sbOut.Append(sText);
                    if (sTag.StartsWith("<bpt"))
                    {
                        sbOut.Append("" + ((char)MARKER_OPENING) + (char)(CHARBASE + id));
                        idStack.Push(id);
                        id++;
                    }
                    else if (sTag.StartsWith("<ept"))
                    {
                        sbOut.Append("" + ((char)MARKER_CLOSING) + (char)(CHARBASE + idStack.Pop()));
                    }
                    else if (sTag.StartsWith("<ph") || sTag.StartsWith("<x") || sTag.StartsWith("<it"))
                    {
                        sbOut.Append("" + ((char)MARKER_ISOLATED) + (char)(CHARBASE + id));
                        id++;
                    }
                }

                sText = sTmxSeg.Substring(prevEnd, sTmxSeg.Length - prevEnd);
                sText = HttpUtility.HtmlDecode(sText); //xml decode
                sbOut.Append(sText);

                return sbOut.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("TmxSegmentToCodedText: " + ex.Message);
            }
            finally
            {
            }
        }

        public static String CodedTextToGoogleTags(string codedText)
        {
            var tagsMap = GetCodesMap(codedText);
            var reversedTagsMap = tagsMap.ToDictionary(x => x.Value, x => x.Key);

            //create simple codes
            var tmp = new StringBuilder();
            for (int i = 0; i < codedText.Length; i++)
            {
                var charCode = codedText[i];
                if (charCode == MARKER_OPENING)
                {
                    tmp.Append("{" + reversedTagsMap[codedText[++i] - CHARBASE] + "}");
                }
                else if (charCode == MARKER_CLOSING)
                {
                    tmp.Append("{/" + reversedTagsMap[codedText[++i] - CHARBASE] + "}");
                }
                else if (charCode == MARKER_ISOLATED)
                {
                    tmp.Append("{" + reversedTagsMap[codedText[++i] - CHARBASE] + "/}");
                }
                else
                    tmp.Append(charCode);
            }

            return tmp.ToString();
        }

        public static Dictionary<int, int> GetCodesMap(String codedText)
        {
            var tagsMap = new Dictionary<int, int>();
            MatchCollection matches = Regex.Matches(codedText, @"{\s*(?<id>\d+)\s*}|{\s*/\s*(?<id>\d+)\s*}|{\s*(?<id>\d+)\s*/\s*}");
            var idsToSkip = new HashSet<String>();
            foreach (Match match in matches)
                idsToSkip.Add(match.Groups["id"].Value);

            var id = 1;
            //create simple codes
            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < codedText.Length; i++)
            {
                while (idsToSkip.Contains(id.ToString()))
                    id++;

                var charCode = codedText[i];
                if (charCode == MARKER_OPENING || charCode == MARKER_ISOLATED)
                {
                    tagsMap.Add(id, codedText[++i] - CHARBASE);
                    id++;
                }
            }

            return tagsMap;
        }

        public static Dictionary<string, string> GetTagsMap(String sXliff, TagType tagType)
        {
            //collect the ids to skip
            var outTagsMap = new Dictionary<string, string>();
            MatchCollection matches = Regex.Matches(sXliff, @"{\s*(?<id>\d+)\s*}|{\s*/\s*(?<id>\d+)\s*}|{\s*(?<id>\d+)\s*/\s*}");
            HashSet<string> idsToSkip = new HashSet<string>();
            foreach (Match match in matches)
                idsToSkip.Add(match.Groups["id"].Value);

            var idAttr = "id";
            if (tagType == TagType.Tmx)
                idAttr = "i";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml("<root>" + sXliff + "</root>");
            var openTags = new Dictionary<string, string>();
            XmlNodeList nodeList = xmlDoc!.ChildNodes![0]!.ChildNodes;
            int id = 1;
            foreach (XmlNode node in nodeList)
            {
                while (idsToSkip.Contains(id.ToString()))
                    id++;
                if (node.NodeType == XmlNodeType.Element)
                {
                    switch (node.Name)
                    {
                        case "ph":
                        case "x":
                            outTagsMap.Add(id + "/", node.OuterXml);
                            id++;
                            break;
                        case "bpt":
                            var sId = id.ToString();
                            outTagsMap.Add(sId, node.OuterXml);
                            openTags.Add(node!.Attributes![idAttr]!.Value, sId);
                            id++;
                            break;
                        case "ept":
                            outTagsMap.Add("/" + openTags[node!.Attributes![idAttr]!.Value], node.OuterXml);
                            break;
                        case "it":
                            outTagsMap.Add(id + "/", node.OuterXml);
                            id++;
                            //cLogManager.DEBUG_LOG("tagErrors.log", "GetXliffTagsMap " + sXliff);
                            break;
                        default:
                            //cLogManager.DEBUG_LOG("tagErrors.log", "GetXliffTagsMap " + sXliff);
                            break;
                    }
                }
            }

            return outTagsMap;
        }


        public static String GoogleTagsToTmx(string googleText, Dictionary<int, int> tagsMap)
        {
            //create simple codes
            String sTmx = googleText;
            MatchCollection matches = Regex.Matches(googleText, @"{\s*(?<id>\d+)\s*}|{\s*/\s*(?<id>\d+)\s*}|{\s*(?<id>\d+)\s*/\s*}");
            foreach (Match match in matches)
            {
                int id = int.Parse(match.Groups["id"].Value);

                if (match.Value.Contains("\\")) //closing tag
                    sTmx = sTmx.Replace(match.Value, "<ept i=\"" + tagsMap[id].ToString() + "\">{}</ept>");
                else if (match.Value.Contains("/")) //isolated tag
                    sTmx = sTmx.Replace(match.Value, "<ph i=\"" + tagsMap[id].ToString() + "\">{}</ph>");
                else //opening tag
                    sTmx = sTmx.Replace(match.Value, "<bpt i=\"" + tagsMap[id].ToString() + "\">{}</bpt>");
            }

            return sTmx;
        }

        public static String GoogleTags2XmlTags(String sGoogleTags, Dictionary<String, String> tagsMap)
        {
            String sRet = sGoogleTags;
            sRet = SecurityElement.Escape(sRet);
            if (tagsMap == null)
                return sRet;

            foreach (String sTag in tagsMap.Keys)
            {
                int id = int.Parse(Regex.Match(sTag, "\\d+").Value);
                if (sTag.StartsWith("/")) //close tag
                    sRet = Regex.Replace(sRet, @"\{\s*/\s*" + id + @"\s*\}", tagsMap[sTag]);
                else if (sTag.EndsWith("/")) //standalone tag
                    sRet = Regex.Replace(sRet, @"\{\s*" + id + @"\s*/\s*\}", tagsMap[sTag]);
                else //opening tag
                    sRet = Regex.Replace(sRet, @"\{\s*" + id + @"\s*}", tagsMap[sTag]);
            }

            return sRet;
        }
    }
}
