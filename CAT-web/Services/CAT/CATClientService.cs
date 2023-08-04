using CATWeb.Data;
using CATWeb.Helpers;
using CATWeb.Models;
using CATService;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Transactions;
using static ICSharpCode.SharpZipLib.Zip.ZipEntryFactory;
using System.Xml;
using CATWeb.Enums;
using CATWeb.Services.MT;
using Microsoft.Extensions.Options;
using CATWeb.Models.CAT;
using Statistics = CATWeb.Models.CAT.Statistics;
using TMMatch = CATWeb.Models.CAT.TMMatch;
using TMAssignment = CATWeb.Models.CAT.TMAssignment;
using AutoMapper;
using System.Security.AccessControl;
using TBEntry = CATWeb.Models.CAT.TBEntry;
using Microsoft.CodeAnalysis.Differencing;
using ICSharpCode.SharpZipLib.Tar;

namespace CATWeb.Services.CAT
{
    public class CATClientService
    {
        private readonly CATWebContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<IMachineTranslator> _machineTranslators;
        private readonly IMapper _mapper;

        private static int MATCH_THRESHOLD = 50;

        /// <summary>
        /// CATClientService
        /// </summary>
        public CATClientService(CATWebContext context, IConfiguration configuration, IEnumerable<IMachineTranslator> machineTranslators,
            IMapper mapper)
        {
            _context = context;
            _configuration = configuration;
            _machineTranslators = machineTranslators;
            _mapper = mapper;
        }

        private EndpointAddress GetCATServiceEndpoint()
        {
            var endPointAddr = "net.tcp://10.0.20.55:8086";

            //local test
            //endPointAddr = "net.tcp://localhost:8086";
            //create the endpoint address
            return new EndpointAddress(endPointAddr);
        }

        /// <summary>
        /// GetCATServiceBinding
        /// </summary>
        /// <returns></returns>
        private static NetTcpBinding GetCATServiceBinding(int timeoutInMinutes)
        {
            TimeSpan timeSpan;
            if (timeoutInMinutes > 0)
                timeSpan = new TimeSpan(0, timeoutInMinutes, 0); // new TimeSpan(timeout * 10000);
            else
                timeSpan = new TimeSpan(0, 5, 0); // 5 minutes

            //set up the binding for the TCP connection
            NetTcpBinding tcpBinding = new NetTcpBinding();

            tcpBinding.Name = "NetTcpBinding_ICATService";
            tcpBinding.CloseTimeout = timeSpan;
            tcpBinding.OpenTimeout = timeSpan;
            tcpBinding.ReceiveTimeout = timeSpan;
            tcpBinding.SendTimeout = timeSpan;
            tcpBinding.TransferMode = TransferMode.Buffered;
            tcpBinding.MaxBufferPoolSize = 524288 * 1000;
            tcpBinding.MaxBufferSize = 65536 * 10000;
            tcpBinding.ReaderQuotas.MaxDepth = 32;
            tcpBinding.ReaderQuotas.MaxStringContentLength = 8192 * 100000;
            tcpBinding.ReaderQuotas.MaxArrayLength = 16384 * 10000;
            tcpBinding.ReaderQuotas.MaxBytesPerRead = 4096000;
            tcpBinding.ReaderQuotas.MaxNameTableCharCount = 1638400000;
            tcpBinding.ReliableSession.Ordered = true;
            tcpBinding.ReliableSession.InactivityTimeout = new TimeSpan(0, 10, 0);
            tcpBinding.ReliableSession.Enabled = false;
            tcpBinding.Security.Mode = SecurityMode.None;

            return tcpBinding;
        }

        private ICATService GetCATService()
        {
            ChannelFactory<ICATService> channelFactory =
                new ChannelFactory<ICATService>(GetCATServiceBinding(5), GetCATServiceEndpoint());

            foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
            }

            return channelFactory.CreateChannel();
        }

        public bool CanBeParsed(String sFilePath)
        {
            //this supposed to be on connector level
            String sExt = Path.GetExtension(sFilePath).ToLower();

            if (sExt == ".doc" || sExt == ".docx" || sExt == ".xls" || sExt == ".xlsx" || sExt == ".ppt" || sExt == ".pptx"
               || sExt == ".htm" || sExt == ".html" || sExt == ".txt" || sExt == ".rtf" || sExt == ".xml" || sExt == ".xlf"
               || sExt == ".xliff" || sExt == ".mqxliff" || sExt == ".sdlxliff" || sExt == ".mqxlz" || sExt == ".xlz" /*|| sExt == ".pdf"*/ || sExt == ".mdd"
               || sExt == ".resx" || sExt == ".strings" || sExt == ".csv" || sExt == ".wsxz"
               || sExt == ".json" || sExt == ".idml" || sExt == ".sdlppx")
                return true;

            return false;
        }

        private String GetDefaultFilter(String sFilePath)
        {
            var sExt = Path.GetExtension(sFilePath).ToLower();
            var sFilterName = "";
            switch (sExt)
            {
                case ".docx":
                case ".xlsx":
                case ".pptx":
                    sFilterName = "okf_openxml@default-okf_openxml.fprm";
                    break;
                default:
                    sFilterName = "";
                    break;
            }

            if (String.IsNullOrEmpty(sFilterName))
                return null;

            var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]);
            var sFilterPath = Path.Combine(fileFiltersFolder, sFilterName);

            return sFilterPath;
        }

        public Models.CAT.Statistics[] GetStatisticsForDocument(string sFilePath, string sFilterPath, String sourceLang,
            string[] aTargetLangs, TMAssignment[] aTMAssignments)
        {
            List<String> lstFilesToDelete = new List<String>();
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                if (!CanBeParsed(sFilePath))
                    throw new Exception("File type cannot be parsed");

                //filter
                if (String.IsNullOrEmpty(sFilterPath))
                    sFilterPath = GetDefaultFilter(sFilePath);

                var client = GetCATService();

                //var aTMSettings = GetTMSettings(1044, idFromLng, aIdTargetLangs, sSpeciality, false);

                if (CATUtils.IsCompressedMemoQXliff(sFilePath))
                {
                    sFilePath = CATUtils.ExtractMQXlz(sFilePath, _configuration["TempFolder"]);
                    lstFilesToDelete.Add(sFilePath);
                }

                //set the parameters
                String sFilename = Path.GetFileName(sFilePath);
                byte[] fileContent = File.ReadAllBytes(sFilePath);
                String sFiltername = Path.GetFileName(sFilterPath);
                byte[]? filterContent = null;
                if (File.Exists(sFilterPath))
                    filterContent = File.ReadAllBytes(sFilterPath);

                var aTMs = _mapper.Map<CATService.TMAssignment[]>(aTMAssignments);

                //the target language array
                var lstTargetLangs = new List<String>();
                var aRet = new List<Statistics>();
                foreach (string sTargetLang in aTargetLangs)
                {
                    var stats = client.GetStatisticsForDocument(sFilename, fileContent, sFiltername, filterContent,
                    sourceLang, new string[] { sTargetLang }, aTMs);
                    aRet.Add(new Statistics()
                    {
                        repetitions = stats[0].repetitions,
                        match_101 = stats[0].match_101,
                        match_100 = stats[0].match_100,
                        match_95_99 = stats[0].match_95_99,
                        match_85_94 = stats[0].match_85_94,
                        match_75_84 = stats[0].match_75_84,
                        match_50_74 = stats[0].match_50_74,
                        no_match = stats[0].no_match,
                        targetLang = sTargetLang
                    });

                }

                sw.Stop();
                return aRet.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //clean-up
                foreach (var tmpFileName in lstFilesToDelete)
                    try
                    {
                        File.Delete(tmpFileName);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
            }
        }

        public void CreateXliffFromDocument(String sTranslationDir,
            String sOutFileName, String sFilePath, String sFilterPath, string sourceLang,
            string targetLang, int iGoodMatchRate)
        {
            try
            {
                //check the filter
                if (String.IsNullOrEmpty(sFilterPath))
                    sFilterPath = GetDefaultFilter(sFilePath);

                var matchThreshold = (iGoodMatchRate >= 50 && iGoodMatchRate <= 101) ? iGoodMatchRate : 99;
                var client = GetCATService();
                //set the parameters
                String sFilename = Path.GetFileName(sFilePath);
                byte[] fileContent = File.ReadAllBytes(sFilePath);

                String? sFiltername = File.Exists(sFilterPath) ? Path.GetFileName(sFilterPath) : null;
                byte[]? filterContent = sFiltername != null ? File.ReadAllBytes(sFilterPath) : null;
                var aTMs = new CATService.TMAssignment[] { new CATService.TMAssignment() { tmPath = "29610/__35462_en_fr" } };
                String sXliffContent = client.CreateXliff(sFilename, fileContent, sFiltername, filterContent, sourceLang, targetLang, aTMs);

                var sOutXliffPath = Path.Combine(sTranslationDir, sOutFileName);
                File.WriteAllText(sOutXliffPath, sXliffContent);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
            }
        }


        protected static bool IsLocked(Object syncRoot)
        {
            bool acquired = false;
            try
            {
                acquired = Monitor.TryEnter(syncRoot);
                return !acquired;
            }
            finally
            {
                if (acquired)
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }

        public static String GetParseDocLockString(int idJob)
        {
            return "ParseDoc_" + idJob.ToString();
        }

        public void ParseDoc(int idJob)
        {
            if (IsLocked(String.Intern(GetParseDocLockString(idJob))))
                throw new Exception("Translation is locked: " + idJob.ToString());

            int nIdx = 1;
            List<String> filesToDelete = new List<String>();
            int iThreshold = 100;

            try
            {
                lock (String.Intern(GetParseDocLockString(idJob))) //lock on the job id
                {

                    //get the translation details
                    var job = _context.Job.Find(idJob);

                    //check if it is parsed already
                    if (job?.DateProcessed != null)
                        throw new Exception("Already processed.");

                    //Get the document
                    var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]);
                    string filePath = Path.Combine(sourceFilesFolder, job.FileName!);
                    var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]);

                    //get the filter
                    string filterPath = "";
                    if (!String.IsNullOrEmpty(job.FilterName))
                        filterPath = Path.Combine(sourceFilesFolder, job.FilterName);

                    var jobDataFolder = CATUtils.GetJobDataFolder(idJob, _configuration["JobDataBaseFolder"]);

                    //var aContentForMT = new List<MTContent>();

                    String sXlifFilePath = CATUtils.CreateXlfFilePath(idJob, DocumentType.original, _configuration["JobDataBaseFolder"]); //we do a backup of the original xliff

                    //pre-process the document
                    String tmpFilePath = null;// DocumentProcessor.PreProcessDocument(sFilePath, idFilter, idFrom, idTo);
                    if (tmpFilePath != null)
                    {
                        filePath = tmpFilePath;
                        filesToDelete.Add(tmpFilePath);
                    }

                    if (CATUtils.IsCompressedMemoQXliff(filePath))
                    {
                        filePath = CATUtils.ExtractMQXlz(filePath, _configuration["TempFolder"]);
                        filesToDelete.Add(filePath);
                    }

                    //get the TMs
                    //TMSetting[] tmSettings = GetTMSettings(idProfile, idFrom, idTo, sSpeciality, true);
                    CreateXliffFromDocument(jobDataFolder, Path.GetFileName(sXlifFilePath), filePath, filterPath,
                        job.SourceLang, job.TargetLang, iThreshold);

                    //parse the xliff file
                    var Xliff = new XmlDocument();
                    Xliff.PreserveWhitespace = true;
                    Xliff.Load(sXlifFilePath);
                    var xmlnsManager = new XmlNamespaceManager(Xliff.NameTable);
                    xmlnsManager.AddNamespace("x", Xliff.DocumentElement.NamespaceURI);

                    var tus = Xliff.GetElementsByTagName("trans-unit");

                    var lstTus = new List<TranslationUnit>();
                    foreach (XmlNode tu in tus)
                    {
                        var tuId = tu.Attributes["id"].Value;
                        var targetNode = tu["target"];
                        XmlNodeList? sourceSegments = null;
                        var ssNode = tu["seg-source"];

                        if (ssNode == null)
                            sourceSegments = tu.SelectNodes("x:source", xmlnsManager);
                        else
                            sourceSegments = ssNode.SelectNodes("x:mrk", xmlnsManager);

                        foreach (XmlNode sourceSegment in sourceSegments!)
                        {
                            var translationUnit = new TranslationUnit();
                            translationUnit.tuid = nIdx;
                            translationUnit.idJob = idJob;

                            if (CATUtils.IsSegmentEmptyOrWhiteSpaceOnly(sourceSegment.InnerXml.Trim()))
                                continue;

                            var mid = "-1";
                            if (sourceSegment.Name == "mrk")
                                mid = sourceSegment.Attributes["mid"].Value;
                            var source = sourceSegment.InnerXml.Trim();

                            int matchQuality = 0;
                            bool bEdited = false;
                            //get the translation
                            var statusAttr = sourceSegment.Attributes["status"];
                            if (statusAttr != null && (statusAttr.Value == "tmEdited" ||
                                statusAttr.Value.StartsWith("tmPreTranslated")))
                            {
                                bEdited = true;
                                if (statusAttr.Value.StartsWith("tmPreTranslated"))
                                {
                                    var val = Regex.Match(statusAttr.Value, "\\d+").Value;
                                    int.TryParse(val, out matchQuality);
                                }
                            }

                            /*bool bLockForTranslators = (segmentLock & 0x1) > 0 && matchQuality >= 100 ||
                                (segmentLock & 0x1000) > 0 && matchQuality >= 101;
                            bool bLockForRevisers = (segmentLock & 0x10) > 0 && matchQuality >= 101 ||
                                (segmentLock & 0x100) > 0 && matchQuality >= 100;
                            if (sourceSegment.Attributes["translate"] != null && sourceSegment.Attributes["translate"].Value == "no")
                                bLockForTranslators = bLockForRevisers = true;*/

                            translationUnit.source = CATUtils.XliffSegmentToCodedText(source);

                            XmlNode? targetSegment = null;
                            if (sourceSegment.Name == "mrk")
                                targetSegment = targetNode.SelectSingleNode("x:mrk[@mid=" + mid + "]", xmlnsManager);
                            else
                                targetSegment = targetNode;
                            if (matchQuality >= 100 || bEdited)
                            {
                                String sTarget = CATUtils.XmlTags2GoogleTags(targetSegment!.InnerXml, CATUtils.TagType.Xliff); //we store the target text with google tags
                                translationUnit.target = sTarget;
                            }
                            lstTus.Add(translationUnit);
                            nIdx++;
                        }
                    }

                    //machine translation
                    var mmt = _machineTranslators.OfType<MMT>().First();

                    //create translatables
                    var translatables = lstTus.Select(tu => new Translatable
                    {
                        id = tu.tuid,
                        source = CATUtils.CodedTextToTmx(tu.source),
                        target = tu.target!
                    }).ToList();

                    //do the machine translation
                    mmt.Translate(translatables, job.SourceLang, job.TargetLang, null);

                    //populate the translation units with the machine translation
                    Dictionary<int, Translatable> translatableDictionary = translatables.ToDictionary(t => t.id);
                    lstTus.ForEach(tu =>
                    {
                        if (translatableDictionary.TryGetValue(tu.tuid, out var translatable))
                        {
                            String targetWithGoogleTags = CATUtils.XmlTags2GoogleTags(translatable.target, CATUtils.TagType.Tmx); //we store the target text with google tags
                            tu.target = targetWithGoogleTags;
                        }
                    });

                    //save the machine translation into the original xliff
                    Xliff.Save(sXlifFilePath);

                    // Add the array of TranslationUnit objects to the DbSet
                    _context.TranslationUnit.AddRange(lstTus);
                    // Save changes in the context to the database
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                //clean-up
                foreach (var tmpFileName in filesToDelete)
                    File.Delete(tmpFileName);
            }
        }

        public TMMatch[] GetTMMatches(TMAssignment[] aTMAssignments, string sSourceXml, string sPrevXml, string sNextXml, string sContextID)
        {
            //we can't send over null value
            var client = GetCATService();
            var atms = _mapper.Map<CATService.TMAssignment[]>(aTMAssignments);

            var matches = client.GetTMMatches(atms, sSourceXml, sPrevXml, sNextXml, (byte)MATCH_THRESHOLD, 10);

            var tmMatches = _mapper.Map<TMMatch[]>(matches);

            //convert and remove duplicates
            var finalTMMatches = new Dictionary<String, TMMatch>();
            foreach (var tmMatch in tmMatches)
            {
                String key = tmMatch.source + tmMatch.target;
                if (finalTMMatches.ContainsKey(key))
                    continue;

                tmMatch.source = CATUtils.XmlTags2GoogleTags(tmMatch.source, CATUtils.TagType.Tmx);
                tmMatch.target = CATUtils.XmlTags2GoogleTags(tmMatch.target, CATUtils.TagType.Tmx);

                finalTMMatches.Add(key, tmMatch);
            }

            return finalTMMatches.Values.ToArray();
        }

        public TMMatch[] GetConcordance(TMAssignment[] aTMAssignments, string sSearchText, bool bCaseSensitive, bool bSearchInTarget)
        {
            //we can't send over null value
            var client = GetCATService();
            var tmPaths = Array.ConvertAll(aTMAssignments, tma => tma.tmPath);
            string? source = null;
            string? target = null;
            if (bSearchInTarget)
                source = sSearchText;
            else
                target = sSearchText;

            var maxHits = 10;
            var tmEntries = client.Concordance(tmPaths, source, target, bCaseSensitive, maxHits);

            //convert and remove duplicates
            var finalTMMatches = new Dictionary<String, TMMatch>();
            foreach (var tmEntry in tmEntries)
            {
                String key = tmEntry.source + tmEntry.target;
                if (finalTMMatches.ContainsKey(key))
                    continue;

                var tmMatch = new TMMatch()
                {
                    id = tmEntry.id,
                    source = CATUtils.XmlTags2GoogleTags(tmEntry.source, CATUtils.TagType.Tmx),
                    target = CATUtils.XmlTags2GoogleTags(tmEntry.target, CATUtils.TagType.Tmx),
                    metadata = tmEntry.metadata
                };

                finalTMMatches.Add(key, tmMatch);
            }

            return finalTMMatches.Values.ToArray();
        }

        public TBEntry[] ListTBEntries(TBAssignment tBAssignment, String[] languages)
        {
            var client = GetCATService();
            //get the TB id from guid
            var tbEntries = client.ListTBEntries(tBAssignment.idTermbase, languages);

            //convert the list
            var lstRet = new List<TBEntry>();
            foreach (var tbEntry in tbEntries)
            {
                lstRet.Add(new TBEntry()
                {
                    id = tbEntry.id,
                    terms = tbEntry.terms,
                    comment = tbEntry.comment,
                    metadata = tbEntry.metadata
                });
            }

            return lstRet.ToArray();
        }

    }
}
