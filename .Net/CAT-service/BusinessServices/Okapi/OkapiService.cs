using CAT.Models;
using CAT.TM;
using CAT.Utils;
using Com.Cat.Grpc;
using Google.Protobuf;
using Grpc.Net.Client;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using static Com.Cat.Grpc.Okapi;

namespace CAT.BusinessServices.Okapi
{
    public class OkapiService : IOkapiService
    {
        private readonly ITMService _tmService;
        private readonly ILogger _logger;
        private readonly IOkapiService _okapiService;
        private readonly IConfiguration _configuration;

        public OkapiService(IOkapiService okapiService, ITMService tmService, IConfiguration configuration, ILogger<OkapiService> logger)
        {
            _okapiService = okapiService;
            _tmService = tmService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// CreateXliffFromDocument
        /// </summary>
        /// <param name="sFileName"></param>
        /// <param name="fileContent"></param>
        /// <param name="sFilterName"></param>
        /// <param name="filterContent"></param>
        /// <param name="sourceLangISO639_1"></param>
        /// <param name="targetLangISO639_1"></param>
        /// <param name="aTMAssignments"></param>
        /// <returns></returns>
        public string CreateXliffFromDocument(string fileName, byte[] fileContent, string filterName, byte[] filterContent,
            string sourceLangISO639_1, string targetLangISO639_1, TMAssignment[] aTMAssignments)
        {
            var xliffContent = CreateXliffFromDocument(fileName, fileContent, filterName,
                filterContent, sourceLangISO639_1, targetLangISO639_1);

            var preTranslatedXliff = PreTranslateXliff(xliffContent, sourceLangISO639_1, targetLangISO639_1, aTMAssignments, 100);

            return preTranslatedXliff;
        }

        public string CreateXliffFromDocument(string fileName, byte[] fileContent, string filterName, byte[] filterContent,
            string sourceLangISO639_1, string targetLangISO639_1)
        {
            var okapiServer = _configuration["OkapiServer"]!.ToString();
            using var channel = GrpcChannel.ForAddress(okapiServer);
            var client = new OkapiClient(channel);
            var request = new CreateXliffFromDocumentRequest
            {
                FileName = fileName,
                FileContent = ByteString.CopyFrom(fileContent),
                FilterContent = ByteString.CopyFrom(filterContent),
                FilterName = filterName,
                SourceLangISO6391 = sourceLangISO639_1,
                TargetLangISO6391 = targetLangISO639_1
            };
            var response = client.CreateXliffFromDocument(request);

            return response.XliffContent;
        }

        /// <summary>
        /// CreateDocumentFromXliff
        /// </summary>
        /// <param name="sFileName"></param>
        /// <param name="fileContent"></param>
        /// <param name="sFilterName"></param>
        /// <param name="filterContent"></param>
        /// <param name="sourceLangISO639_1"></param>
        /// <param name="targetLangISO639_1"></param>
        /// <param name="sXliffContent"></param>
        /// <returns></returns>
        public byte[] CreateDocumentFromXliff(string fileName, byte[] fileContent, string filterName, byte[] filterContent,
            string sourceLangISO639_1, string targetLangISO639_1, string xliffContent)
        {
            var okapiServer = _configuration["OkapiServer"]!.ToString();
            using var channel = GrpcChannel.ForAddress(okapiServer);
            var client = new OkapiClient(channel);
            var request = new CreateDocumentFromXliffRequest
            {
                FileName = fileName,
                FileContent = ByteString.CopyFrom(fileContent),
                FilterContent = ByteString.CopyFrom(filterContent),
                FilterName = filterName,
                SourceLangISO6391 = sourceLangISO639_1,
                TargetLangISO6391 = targetLangISO639_1,
                XliffContent = xliffContent,
            };
            var response = client.CreateDocumentFromXliff(request);

            return response.CreatedDocument.ToByteArray();
        }

        /// <summary>
        /// PreTranslateXliff
        /// </summary>
        /// <param name="sXliffContent"></param>
        /// <param name="langFrom_ISO639_1"></param>
        /// <param name="langTo_ISO639_1"></param>
        /// <param name="aTMAssignments"></param>
        /// <param name="matchThreshold"></param>
        /// <returns></returns>
        public string PreTranslateXliff(string sXliffContent, string langFrom_ISO639_1, string langTo_ISO639_1,
            TMAssignment[] aTMAssignments, int matchThreshold)
        {
            int cntr = 0;
            try
            {
                //check the languages for the TMs
                if (aTMAssignments != null)
                {
                    var lstTmpTMs = new List<TMAssignment>();
                    foreach (var tmAssignment in aTMAssignments)
                    {
                        var tmInfo = _tmService.GetTMInfo(tmAssignment.tmId, false);
                        if (tmInfo.langFrom?.ToLower() == langFrom_ISO639_1?.ToLower() && tmInfo.langTo?.ToLower() == langTo_ISO639_1?.ToLower())
                            lstTmpTMs.Add(tmAssignment);
                    }
                    aTMAssignments = lstTmpTMs.ToArray();
                }
                else
                    aTMAssignments = new TMAssignment[0];

                //load the xliff file as xml
                XmlDocument xliff = new XmlDocument();
                xliff.PreserveWhitespace = true;
                xliff.LoadXml(sXliffContent);

                XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xliff.NameTable);
                xmlnsManager.AddNamespace("x", xliff.DocumentElement!.NamespaceURI);

                //fix the source language
                var fileNode = xliff.SelectSingleNode("//x:file", xmlnsManager);
                fileNode!.Attributes!["source-language"]!.Value = langFrom_ISO639_1;

                XmlNodeList tuNodeList = xliff.GetElementsByTagName("trans-unit");
                string prev = default!;
                string next = default!;
                for (int i = 0; i < tuNodeList.Count; i++)
                {
                    cntr = i;
                    Stopwatch swInner = new Stopwatch();
                    swInner.Start();
                    XmlNode tuNode = tuNodeList[i]!;

                    //create the target node if it doesn't exists
                    var targetNode = tuNode!["target"];
                    if (targetNode == null)
                    {
                        targetNode = xliff.CreateElement("target", xliff.DocumentElement.NamespaceURI);
                        //the target language
                        var xmlAttribute = xliff.CreateAttribute("xml:lang");
                        xmlAttribute.Value = langTo_ISO639_1;
                        targetNode.Attributes.Append(xmlAttribute);
                        tuNode.AppendChild(targetNode);
                    }

                    XmlNodeList segmentNodes = default!;
                    var ssNode = tuNode["seg-source"];
                    if (ssNode == null)
                        segmentNodes = tuNode!.SelectNodes("x:source", xmlnsManager)!;
                    else
                        segmentNodes = ssNode!.SelectNodes("x:mrk", xmlnsManager)!;
                    var targetXml = "";
                    for (int j = 0; j < segmentNodes!.Count; j++)
                    {
                        XmlNode segmentNode = segmentNodes[j]!;
                        var source = segmentNode!.InnerXml.Trim();
                        var status = segmentNode!.Attributes!["status"];
                        if (status == null)
                        {
                            status = xliff.CreateAttribute("status");
                            segmentNode.Attributes.Append(status);
                        }

                        var mid = "-1";
                        if (segmentNode.Name == "mrk")
                            mid = segmentNode!.Attributes!["mid"]!.Value;
                        //get the next segment for the context
                        if (j < segmentNodes.Count - 1)
                            next = segmentNodes![j + 1]!.InnerXml.Trim();
                        else
                        {
                            //pick the first segment from the next tu
                            var nextTu = i < tuNodeList.Count - 1 ? tuNodeList[i + 1] : null;
                            if (segmentNode.Name == "mrk")
                                next = nextTu?["seg-source"]?["mrk"]?.InnerXml.Trim()!;
                            else
                                next = nextTu?["source"]!.InnerXml.Trim()!;
                        }

                        XmlNode targetSegment = default!;
                        if (segmentNode.Name == "mrk")
                            targetSegment = targetNode!.SelectSingleNode("x:mrk[@mid='" + mid + "']", xmlnsManager)!;
                        else
                            targetSegment = targetNode;
                        if (targetSegment == null || targetSegment.InnerXml.Length == 0)
                        {
                            String sStartingWhiteSpaces = Regex.Match(segmentNode.InnerXml, @"^\s*").Value;
                            string sEndingWhiteSpaces = Regex.Match(segmentNode.InnerXml, @"\s*$").Value;
                            //the translation
                            var tmMatch = _tmService.GetExactMatch(aTMAssignments, source, prev!, next!);
                            //set the translation
                            if (tmMatch != null && tmMatch.quality >= 100)
                            {
                                var xlfTarget = CATUtils.TextFragmentToXliff(CATUtils.TmxSegmentToTextFragmentSimple(tmMatch.target));
                                if (segmentNode.Name == "mrk")
                                    targetXml += "<mrk mid=\"" + mid + "\" mtype=\"seg\">" + sStartingWhiteSpaces +
                                        xlfTarget + sEndingWhiteSpaces + "</mrk>";
                                else
                                    targetXml = sStartingWhiteSpaces + xlfTarget + sEndingWhiteSpaces;
                                status.Value = "tmPreTranslated:" + tmMatch.quality;
                            }
                            else
                            {
                                if (segmentNode.Name == "mrk")
                                    targetXml += "<mrk mid=\"" + mid + "\" mtype=\"seg\"></mrk>";
                                status.Value = "tmNotStarted";
                            }
                        }
                        else
                        {
                            if (segmentNode.Name == "mrk")
                                targetXml += "<mrk mid=\"" + mid + "\" mtype=\"seg\">" + targetSegment.InnerXml + "</mrk>";
                            else
                                targetXml = targetSegment.InnerXml;
                            status.Value = "tmEdited";
                        }
                        prev = source;
                    }

                    //set the target
                    targetNode.InnerXml = targetXml;
                }

                return xliff.OuterXml;
            }
            catch (Exception ex)
            {
                _logger.LogError("PreTranslate.log", "ERROR: " + ex.ToString());
                throw new Exception(ex.Message);
            }
            finally
            {
            }
        }
    }
}
