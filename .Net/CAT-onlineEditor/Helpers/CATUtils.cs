﻿using CAT.Helpers;
using CAT.Enums;
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
    public partial class CATUtils
    {
        /// <summary>
        /// IsCompressedMemoQXliff
        /// </summary>
        /// <param name="sFilePath"></param>
        /// <returns></returns>
        public static bool IsCompressedMemoQXliff(string sFilePath)
        {
            string sExt = Path.GetExtension(sFilePath).ToLower();
            if (sExt == ".mqxlz" || sExt == ".xlz")
                return true;

            return false;
        }

        public static string ExtractMQXlz(string sFilePath, string tempFolder)
        {
            string sNewXlfPath = Path.Combine(tempFolder, Guid.NewGuid().ToString() + ".xlf");
            try
            {
                string sExt = Path.GetExtension(sFilePath).ToLower();
                //unzip the file if it is zipped
                if (sExt == ".xlz" || sExt == ".mqxlz")
                {
                    string sTmpDir = Path.Combine(tempFolder, Guid.NewGuid().ToString());
                    ZipHelper.UnZipFiles(sFilePath, sTmpDir, null);
                    //copy the file into the out dir
                    if (sExt == ".xlz")
                    {
                        string[] aFiles = Directory.GetFiles(sTmpDir, "*.xlf");
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

        public static string GetJobDataFolder(int jobId, string jobDataBaseFolder)
        {
            var jobDataFolder = Path.Combine(jobDataBaseFolder, jobId.ToString());

            //create the folder if it doesn't exists
            if (!Directory.Exists(jobDataFolder))
                Directory.CreateDirectory(jobDataFolder);

            return jobDataFolder;
        }

        public static string CreateXlfFilePath(int jobId, DocumentType documentType, string jobDataBaseFolder)
        {
            var jobDataFolder = GetJobDataFolder(jobId, jobDataBaseFolder);
            var xliffPath = Path.Combine(jobDataFolder, documentType.ToString() + ".xliff");
            if (File.Exists(xliffPath))
            {
                //do a backup
                File.Copy(xliffPath, Path.Combine(jobDataFolder, Path.GetFileNameWithoutExtension(xliffPath) + "_" +
                    DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xlf"));
            }

            return xliffPath;
        }

        public static bool IsSegmentEmptyOrWhiteSpaceOnly(string sXml)
        {
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            if (!sXml.StartsWith("<seg>"))
                sXml = "<seg>" + sXml + "</seg>";
            xmlDoc.LoadXml(sXml);
            XmlNodeList nodeList = xmlDoc.ChildNodes?[0]!.ChildNodes!;

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

                var matces = WhiteSpaceOrEmptyRegex().Match(sbTextContent.ToString());
                return matces.Length > 0;
            }
            catch (Exception)
            {
                //clean-up
            }

            return false;
        }

        public static string XliffTags2TMXTags(string sXliffContent)
        {
            string sConverted = sXliffContent;
            sConverted = XPlaceholderRegex().Replace(sConverted, "<ph type='fmt'>{}</ph>"); //no x element in TMX 
            sConverted = IdRegex().Replace(sConverted, "i"); //regex seems to be faster than xslt
            sConverted = CTypeRegex().Replace(sConverted, "type");
            sConverted = TypeUnderlinedRegex().Replace(sConverted, "type=\"ulined\"");
            sConverted = sConverted.Replace(" xmlns=\"urn:oasis:names:tc:xliff:document:1.2\"", ""); //is there better solution?

            return sConverted;
        }

        public enum TagType { Xliff, Tmx };

        public static string XmlTags2GoogleTags(string sXml, TagType tagType)
        {
            //collect the ids to skip
            var sRet = new StringBuilder();
            MatchCollection matches = GoogleTagsRegex().Matches(sXml);
            var idsToSkip = new HashSet<string>();
            foreach (Match match in matches.Cast<Match>())
                idsToSkip.Add(match.Groups["id"].Value);

            string sIdAttr = "id";
            if (tagType == TagType.Tmx)
                sIdAttr = "i";

            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            if (!sXml.StartsWith("<seg>"))
                sXml = "<seg>" + sXml + "</seg>";
            xmlDoc.LoadXml(sXml);
            XmlNodeList nodeList = xmlDoc?.ChildNodes?[0]?.ChildNodes!;
            var openTags = new Dictionary<string, string>();
            int id = 1;
            try
            {
                foreach (XmlNode node in nodeList)
                {
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
                                openTags?.Add(node?.Attributes?[sIdAttr]?.Value!, id.ToString());
                                id++;
                                break;
                            case "ept":
                                sRet.Append("{/" + openTags?[node?.Attributes?[sIdAttr]?.Value!]! + "}");
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
        public static readonly string MARKERS_REGEX = "[\uE101\uE102\uE103\uE104].";

        public static string XliffSegmentToCodedText(string sXliffSegment)
        {
            try
            {
                sXliffSegment ??= "";

                // remove the outer tag
                var sbOut = new StringBuilder();

                // matcher for the tmx tags
                var matches = TmxTagsRegex().Matches(sXliffSegment);
                int id = 1;
                int prevEnd = 0;
                var idStack = new Stack<int>();
                string sText = "";
                foreach (var match in matches.Cast<Match>())
                {
                    string sTag = match.Value.ToLower();
                    sText = sXliffSegment[prevEnd..match.Index];
                    sText = HttpUtility.HtmlDecode(sText); //xml decode
                    prevEnd = match.Index + match.Length;
                    sbOut.Append(sText);
                    if (sTag.StartsWith("<bpt"))
                    {
                        sbOut.Append("" + (char)MARKER_OPENING + (char)(CHARBASE + id));
                        idStack.Push(id);
                        id++;
                    }
                    else if (sTag.StartsWith("<ept"))
                    {
                        sbOut.Append("" + (char)MARKER_CLOSING + (char)(CHARBASE + idStack.Pop()));
                    }
                    else if (sTag.StartsWith("<ph") || sTag.StartsWith("<x"))
                    {
                        sbOut.Append("" + (char)MARKER_ISOLATED + (char)(CHARBASE + id));
                        id++;
                    }
                }

                sText = sXliffSegment[prevEnd..];
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
                //clean-up
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
                    tmp.Append("<bpt id=\"" + (codedText[++i] - CHARBASE).ToString() + "\">{}</bpt>");
                else if (charCode == MARKER_CLOSING)
                    tmp.Append("<ept id=\"" + (codedText[++i] - CHARBASE).ToString() + "\">{}</ept>");
                else if (charCode == MARKER_ISOLATED)
                    tmp.Append("<ph id=\"" + (codedText[++i] - CHARBASE).ToString() + "\">{}</ph>");
                else
                {
                    //xml escape 
                    switch (charCode)
                    {
                        case '>': tmp.Append("&gt;"); break;
                        case '<': tmp.Append("&lt;"); break;
                        case '\r': tmp.Append("&#13;"); break; // Not a line-break in the XML context, but a literal
                        case '&': tmp.Append("&amp;"); break;
                        //case '"': tmp.Append("&quot;"
                        //case '\'': tmp.Append("&apos;"; //"&#39;"
                        default:
                            tmp.Append(charCode); break;
                    }
                }
            }

            return tmp.ToString();
        }

        public static string CodedTextToTmx(string codedText)
        {
            //create simple codes
            var tmp = new StringBuilder();
            for (int i = 0; i < codedText.Length; i++)
            {
                var charCode = codedText[i];
                if (charCode == MARKER_OPENING)
                    tmp.Append("<bpt i=\"" + (codedText[++i] - CHARBASE).ToString() + "\">{}</bpt>");
                else if (charCode == MARKER_CLOSING)
                    tmp.Append("<ept i=\"" + (codedText[++i] - CHARBASE).ToString() + "\">{}</ept>");
                else if (charCode == MARKER_ISOLATED)
                    tmp.Append("<ph i=\"" + (codedText[++i] - CHARBASE).ToString() + "\">{}</ph>");
                else
                {
                    //xml escape 
                    switch (charCode)
                    {
                        case '>': tmp.Append("&gt;"); break;
                        case '<': tmp.Append("&lt;"); break;
                        case '\r': tmp.Append("&#13;"); break; // Not a line-break in the XML context, but a literal
                        case '&': tmp.Append("&amp;"); break;
                        //case '"': tmp.Append("&quot;"
                        //case '\'': tmp.Append("&apos;"; //"&#39;"
                        default:
                            tmp.Append(charCode); break;
                    }
                }
            }

            return tmp.ToString();
        }

        public static string TmxSegmentToCodedText(string sTmxSeg)
        {
            try
            {
                if (string.IsNullOrEmpty(sTmxSeg))
                    return "";

                // remove the outer tag
                var sbOut = new StringBuilder();

                // matcher for the tmx tags
                var matches = XmlTagsRegex().Matches(sTmxSeg);
                int id = 1;
                int prevEnd = 0;
                var idStack = new Stack<int>();
                string sText = "";
                foreach (Match match in matches.Cast<Match>())
                {
                    string sTag = match.Value.ToLower();
                    sText = sTmxSeg[prevEnd..match.Index];
                    sText = HttpUtility.HtmlDecode(sText); //xml decode
                    prevEnd = match.Index + match.Length;
                    sbOut.Append(sText);
                    if (sTag.StartsWith("<bpt"))
                    {
                        sbOut.Append("" + (char)MARKER_OPENING + (char)(CHARBASE + id));
                        idStack.Push(id);
                        id++;
                    }
                    else if (sTag.StartsWith("<ept"))
                    {
                        sbOut.Append("" + (char)MARKER_CLOSING + (char)(CHARBASE + idStack.Pop()));
                    }
                    else if (sTag.StartsWith("<ph") || sTag.StartsWith("<x") || sTag.StartsWith("<it"))
                    {
                        sbOut.Append("" + (char)MARKER_ISOLATED + (char)(CHARBASE + id));
                        id++;
                    }
                }

                sText = sTmxSeg[prevEnd..];
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
                //clean-up
            }
        }

        public static string CodedTextToGoogleTags(string codedText)
        {
            var tagsMap = GetTagsMap(codedText);
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

        public static Dictionary<int, int> GetTagsMap(string codedText)
        {
            var tagsMap = new Dictionary<int, int>();
            MatchCollection matches = GoogleTagsRegex().Matches(codedText);
            var idsToSkip = new HashSet<string>();
            foreach (Match match in matches.Cast<Match>())
                idsToSkip.Add(match.Groups["id"].Value);

            var id = 1;
            //create simple codes
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

        public static string GoogleTagsToTmx(string googleText, Dictionary<int, int> tagsMap)
        {
            //create simple codes
            string sTmx = googleText;
            MatchCollection matches = GoogleTagsRegex().Matches(googleText);
            foreach (Match match in matches.Cast<Match>())
            {
                int id = int.Parse(match.Groups["id"].Value);

                if (match.Value.Contains('\\')) //closing tag
                    sTmx = sTmx.Replace(match.Value, "<ept i=\"" + tagsMap[id].ToString() + "\">{}</ept>");
                else if (match.Value.Contains('/')) //isolated tag
                    sTmx = sTmx.Replace(match.Value, "<ph i=\"" + tagsMap[id].ToString() + "\">{}</ph>");
                else //opening tag
                    sTmx = sTmx.Replace(match.Value, "<bpt i=\"" + tagsMap[id].ToString() + "\">{}</bpt>");
            }

            return sTmx;
        }

        [GeneratedRegex("\\A\\s*\\z")]
        private static partial Regex WhiteSpaceOrEmptyRegex();

        [GeneratedRegex("<x id=['\"].*?['\"].*?/>")]
        private static partial Regex XPlaceholderRegex();

        [GeneratedRegex("(?<=<[^>]*?)(id)")]
        private static partial Regex IdRegex();

        [GeneratedRegex("(?<=<[^>]*?)(ctype)")]
        private static partial Regex CTypeRegex();

        [GeneratedRegex("(?<=<[^>]*?)(type=\"underlined\")")]
        private static partial Regex TypeUnderlinedRegex();

        [GeneratedRegex("<[^>]*>[^>]*>")]
        private static partial Regex XmlTagsRegex();

        [GeneratedRegex("{\\s*(?<id>\\d+)\\s*}|{\\s*/\\s*(?<id>\\d+)\\s*}|{\\s*(?<id>\\d+)\\s*/\\s*}")]
        private static partial Regex GoogleTagsRegex();

        [GeneratedRegex("<[^>]*>[^>]*>")]
        private static partial Regex TmxTagsRegex();
    }
}
