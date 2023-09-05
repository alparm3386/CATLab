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
using CAT.Enums;
using CAT.Services.MT;
using Microsoft.Extensions.Options;
using CAT.Models.Common;
using Statistics = CAT.Models.Common.Statistics;
using TMAssignment = CAT.Models.Common.TMAssignment;
using AutoMapper;
using System.Security.AccessControl;
using Microsoft.CodeAnalysis.Differencing;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;
using CAT.Data;
using CAT.Areas.Identity.Data;
using CATService;
using CAT.Helpers;
using Microsoft.EntityFrameworkCore;
using CAT.Models.Entities.TranslationUnits;
using CAT.Models;

namespace CAT.Services.Common
{
    public class CATConnector
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<IMachineTranslator> _machineTranslators;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IDocumentProcessor _documentProcessor;

        //private static int MATCH_THRESHOLD = 50;

        /// <summary>
        /// CATClientService
        /// </summary>
        public CATConnector(DbContextContainer dbContextContainer, IConfiguration configuration, IEnumerable<IMachineTranslator> machineTranslators, 
            IMapper mapper, ILogger<CATConnector> logger, IDocumentProcessor documentProcessor)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _machineTranslators = machineTranslators;
            _mapper = mapper;
            _logger = logger;
            _documentProcessor = documentProcessor;
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
                return null!;

            var fileFiltersFolder = Path.Combine(_configuration!["FileFiltersFolder"]!);
            var sFilterPath = Path.Combine(fileFiltersFolder, sFilterName);

            return sFilterPath;
        }

        public Statistics[] GetStatisticsForDocument(string sFilePath, string sFilterPath, String sourceLang,
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

                if (CATUtils.IsCompressedMemoQXliff(sFilePath))
                {
                    sFilePath = CATUtils.ExtractMQXlz(sFilePath, _configuration!["TempFolder"]!);
                    lstFilesToDelete.Add(sFilePath);
                }

                //set the parameters
                String sFilename = Path.GetFileName(sFilePath);
                byte[] fileContent = File.ReadAllBytes(sFilePath);
                String sFiltername = Path.GetFileName(sFilterPath);
                byte[]? filterContent = null;
                if (File.Exists(sFilterPath))
                    filterContent = File.ReadAllBytes(sFilterPath);

                //var aTMs = _mapper.Map<CATService.TMAssignment[]>(aTMAssignments);
                var aTMs = Array.ConvertAll(aTMAssignments, 
                    tma => new CATService.TMAssignment() { penalty = tma.penalty, speciality = tma.speciality, tmPath = tma.tmId });

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
            catch (Exception)
            {
                throw;
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
            catch (Exception)
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
                    var job = _dbContextContainer.MainContext.Jobs.Include(j => j.Quote).Include(j => j.Order).FirstOrDefault(j => j.Id == idJob);
                    var document = _dbContextContainer.MainContext.Documents.Find(job!.SourceDocumentId);

                    //check if it is parsed already
                    if (job?.DateProcessed != null)
                        throw new Exception("Already processed.");

                    //Get the document
                    var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]!);
                    string filePath = Path.Combine(sourceFilesFolder, document!.FileName!);
                    var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]!);

                    //get the filter
                    string filterPath = "";
                    //if (!String.IsNullOrEmpty(job.FilterName))
                    //    filterPath = Path.Combine(sourceFilesFolder, job.FilterName);

                    var jobDataFolder = CATUtils.GetJobDataFolder(idJob, _configuration["JobDataBaseFolder"]!);

                    //var aContentForMT = new List<MTContent>();

                    String sXlifFilePath = CATUtils.CreateXlfFilePath(idJob, DocumentType.Original, _configuration["JobDataBaseFolder"]!, true); //we do a backup of the original xliff

                    //pre-process the document
                    String tmpFilePath = null!;// DocumentProcessor.PreProcessDocument(sFilePath, idFilter, idFrom, idTo);
                    if (tmpFilePath != null)
                    {
                        filePath = tmpFilePath;
                        filesToDelete.Add(tmpFilePath);
                    }

                    if (CATUtils.IsCompressedMemoQXliff(filePath))
                    {
                        filePath = CATUtils.ExtractMQXlz(filePath, _configuration["TempFolder"]!);
                        filesToDelete.Add(filePath);
                    }

                    //get the TMs
                    var aTMAssignments = GetTMAssignments(job!.Order!.ClientId, job!.Quote!.SourceLanguage!,
                        new string[] { job!.Quote!.TargetLanguage! }, job.Quote.Speciality, true);
                    CreateXliffFromDocument(jobDataFolder, Path.GetFileName(sXlifFilePath), filePath, filterPath,
                        job!.Quote!.SourceLanguage!, job!.Quote!.TargetLanguage!, iThreshold);

                    //parse the xliff file
                    var xliff = new XmlDocument();
                    xliff.PreserveWhitespace = true;
                    xliff.Load(sXlifFilePath);
                    var xmlnsManager = new XmlNamespaceManager(xliff.NameTable);
                    xmlnsManager.AddNamespace("x", xliff!.DocumentElement!.NamespaceURI);

                    var tus = xliff.GetElementsByTagName("trans-unit");

                    var lstTus = new List<TranslationUnit>();
                    foreach (XmlNode tu in tus)
                    {
                        var tuId = tu!.Attributes!["id"]!.Value;
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
                                mid = sourceSegment!.Attributes!["mid"]!.Value;
                            var source = sourceSegment.InnerXml.Trim();

                            int matchQuality = 0;
                            bool bEdited = false;
                            //get the translation
                            var statusAttr = sourceSegment!.Attributes!["status"];
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
                                targetSegment = targetNode!.SelectSingleNode("x:mrk[@mid=" + mid + "]", xmlnsManager);
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
                        source = CATUtils.CodedTextToGoogleTags(tu!.source!),
                        target = tu.target!
                    }).ToList();

                    //do the machine translation
                    mmt.Translate(translatables, job!.Quote!.SourceLanguage!, job!.Quote!.TargetLanguage!, null);

                    //populate the translation units with the machine translation
                    Dictionary<int, Translatable> translatableDictionary = translatables.ToDictionary(t => t.id);
                    lstTus.ForEach(tu =>
                    {
                        if (translatableDictionary.TryGetValue(tu.tuid, out var translatable))
                        {
                            tu.target = translatable!.target!;
                        }
                    });

                    //save the machine translation into the original xliff
                    xliff.Save(sXlifFilePath);

                    // Add the array of TranslationUnit objects to the DbSet
                    _dbContextContainer.TranslationUnitsContext.TranslationUnit.AddRange(lstTus);
                    // Save changes in the context to the database
                    _dbContextContainer.TranslationUnitsContext.SaveChanges();

                    job.DateProcessed = DateTime.Now;
                    _dbContextContainer.MainContext.SaveChanges();
                }
            }
            catch (Exception)
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

        public FileData CreateDoc(int idJob, string userId, bool updateTM)
        {
            var lstFilesToDelete = new List<String>();
            try
            {
                //get the service
                var client = GetCATService();
                var jobDataFolder = CATUtils.GetJobDataFolder(idJob, _configuration["JobDataBaseFolder"]!);
                int iThreshold = 100;

                //get the job details
                var job = _dbContextContainer.MainContext.Jobs.Include(j => j.Quote).FirstOrDefault(j => j.Id == idJob);

                //get the document
                var document = _dbContextContainer.MainContext.Documents.Find(job!.SourceDocumentId);
                var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]!);
                string filePath = Path.Combine(sourceFilesFolder, document!.FileName!);

                //get the filter
                var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]!);

                //get the filter
                string filterPath = "";
                //if (!String.IsNullOrEmpty(job.FilterName))
                //    filterPath = Path.Combine(sourceFilesFolder, job.FilterName);

                //pre-process the document
                String tmpFilePath = _documentProcessor.PreProcessDocument(filePath, filterPath);
                if (tmpFilePath != null)
                {
                    filePath = tmpFilePath;
                    lstFilesToDelete.Add(tmpFilePath);
                }

                var sourceLanguage = job!.Quote!.SourceLanguage!;
                var targetLanguage = job!.Quote!.TargetLanguage!;
                var xlifFilePath = CATUtils.CreateXlfFilePath(idJob, DocumentType.Original, _configuration["JobDataBaseFolder"]!, false);
                if (!File.Exists(xlifFilePath))
                {
                    if (CATUtils.IsCompressedMemoQXliff(filePath))
                    {
                        filePath = CATUtils.ExtractMQXlz(filePath, _configuration!["TempFolder"]!);
                        lstFilesToDelete.Add(filePath);
                    }

                    //create the xliff file
                    CreateXliffFromDocument(jobDataFolder, Path.GetFileName(xlifFilePath), filePath, filterPath,
                        sourceLanguage, targetLanguage, iThreshold);
                }

                //get the translated texts
                var translationUnits = _dbContextContainer.TranslationUnitsContext.TranslationUnit
                                 .Where(tu => tu.idJob == idJob).OrderBy(tu => tu.tuid).ToList();

                //fill the xliff file with the translations
                XmlDocument xlfFile = new XmlDocument();
                xlfFile.PreserveWhitespace = true;
                xlfFile.Load(xlifFilePath);
                var xmlnsManager = new XmlNamespaceManager(xlfFile.NameTable);
                xmlnsManager.AddNamespace("x", xlfFile.DocumentElement!.NamespaceURI);
                var transUnits = xlfFile.SelectNodes("//x:trans-unit", xmlnsManager);
                var tmEntries = new List<CATService.TMEntry>();
                int tuid = 1;
                foreach (XmlNode tu in transUnits!)
                {
                    //var tuId = tu.Attributes["id"].Value;
                    var targetNode = tu["target"];
                    XmlNodeList? sourceSegments = null;
                    var ssNode = tu["seg-source"];
                    if (ssNode == null)
                        sourceSegments = tu.SelectNodes("x:source", xmlnsManager);
                    else
                        sourceSegments = ssNode.SelectNodes("x:mrk", xmlnsManager);
                    foreach (XmlNode sourceSegment in sourceSegments!)
                    {
                        if (sourceSegment.InnerXml.Trim().Length == 0)
                        {
                            if (sourceSegment.Name == "mrk")
                            {
                                var mid = sourceSegment!.Attributes!["mid"]!.Value;
                                var tmpSegment = targetNode!.SelectSingleNode("x:mrk[@mid=" + mid + "]", xmlnsManager);
                                tmpSegment!.InnerXml = sourceSegment.InnerXml;
                            }
                            else
                                targetNode!.InnerXml = sourceSegment.InnerXml;
                            continue;
                        }

                        var sStartingWhiteSpaces = Regex.Match(sourceSegment.InnerXml, @"^\s*").Value;
                        var sEndingWhiteSpaces = Regex.Match(sourceSegment.InnerXml, @"\s*$").Value;
                        //confirm the segment
                        XmlAttribute attr = sourceSegment!.Attributes!["status"]!;
                        if (attr != null)
                            attr.Value = "ManuallyConfirmed";

                        XmlNode? targetSegment = null;
                        if (sourceSegment.Name == "mrk")
                        {
                            var mid = sourceSegment!.Attributes["mid"]!.Value!;
                            targetSegment = targetNode!.SelectSingleNode("x:mrk[@mid=" + mid + "]", xmlnsManager);
                            if (targetSegment == null)
                            {
                                //create new segment
                                targetSegment = xlfFile.CreateElement("mrk");
                                var tmpAttr = xlfFile.CreateAttribute("mid");
                                tmpAttr.Value = mid;
                                targetSegment!.Attributes!.Append(tmpAttr);
                                tmpAttr = xlfFile.CreateAttribute("mtype");
                                tmpAttr.Value = "seg";
                                targetSegment.Attributes.Append(tmpAttr);
                                targetNode.AppendChild(targetSegment);
                            }
                        }
                        else
                            targetSegment = targetNode;

                        //get the translation
                        var dbTu = translationUnits[tuid - 1];

                        var sTranslatedText = dbTu.target;

                        //convert the tags
                        Dictionary<String, String> tagsMap = CATUtils.GetTagsMap(sourceSegment.InnerXml, CATUtils.TagType.Xliff);
                        //convert google tags to xliff tags
                        sTranslatedText = CATUtils.GoogleTags2XmlTags(sTranslatedText!, tagsMap);
                        targetSegment!.InnerXml = sStartingWhiteSpaces + sTranslatedText + sEndingWhiteSpaces;

                        //create a TMEntry for the segment
                        var tmEntry = new CATService.TMEntry();
                        tmEntry.source = CATUtils.XliffTags2TMXTags(sourceSegment.InnerXml);
                        tmEntry.target = CATUtils.XliffTags2TMXTags(sTranslatedText);

                        tmEntries.Add(tmEntry);
                        tuid++;
                    }
                }

                if (translationUnits.Count != tuid - 1)
                    throw new Exception("Segments don't match.");


                //update the TMs
                /*if (updateTM)
                {
                    //prepare the TM entries
                    for (int i = 0; i < tmEntries.Count; i++)
                    {
                        var metadata = new Dictionary<String, String>();
                        if (user != null)
                            metadata.Add("user", (int)user.m_utProfile + "_" + user.m_iUserID.ToString());
                        metadata.Add("speciality", translation.idSpeciality.ToString());
                        metadata.Add("idTranslation", idTranslation.ToString());

                        //preceding segment
                        if (i > 0)
                            metadata.Add("prevSegment", tmEntries[i - 1].source);
                        //following segment 
                        if (i < tmEntries.Count - 1)
                            metadata.Add("nextSegment", tmEntries[i + 1].source);

                        tmEntries[i].metadata = JsonConvert.SerializeObject(metadata);
                    }

                    //update the TMs
                    var TMs = GetTMAssignments(1044, job!.Quote!.SourceLanguage, new string[] { job!.Quote!.TargetLanguage! },
                        job!.Quote!.Speciality, true);
                    foreach (var tm in TMs)
                    {
                        if (!tm.isReadonly)
                            client.AddTMEntries(tm.name, tmEntries.ToArray());
                    }
                }*/

                //save the intermediate xlf file.
                var docType = DocumentType.Unspecified;
                /*if (intermediateFileType == IntermediateFileType.currentTask)
                {
                    //get the current doc type
                    var task = (DATA.DataTypes.Task)oWM.GetCurrentWorkflowStep().idTask;
                    docType = DATA.DataTypes.GetDocumentTypeByTask(task);
                }
                else if (intermediateFileType == IntermediateFileType.rectified)
                    docType = DATA.DataTypes.documentType.rectifiedDocument;
                else if (intermediateFileType == IntermediateFileType.adjudication)
                    docType = DATA.DataTypes.documentType.adjudicatedDocument;
                else if (intermediateFileType == IntermediateFileType.reconciliation)
                    docType = DATA.DataTypes.documentType.reconciledDocument;*/

                String sXlfFilePath = CATUtils.CreateXlfFilePath(idJob, docType, _configuration["JobDataBaseFolder"]!, true);
                xlfFile.Save(sXlfFilePath);

                //create the document
                String sFilename = Path.GetFileName(filePath);
                byte[] fileContent = File.ReadAllBytes(filePath);
                String? sFiltername = null;
                byte[]? filterContent = null;
                if (String.IsNullOrEmpty(filterPath))
                    filterPath = GetDefaultFilter(filePath);

                if (!String.IsNullOrEmpty(filterPath))
                {
                    sFiltername = Path.GetFileName(filterPath);
                    filterContent = File.ReadAllBytes(filterPath);
                }

                //create the file
                byte[]? aOutFileBytes = null;
                var fileExtension = Path.GetExtension(filePath).ToLower();
                if (filterPath != null && Path.GetExtension(filterPath).ToLower() == ".mqres" || Path.GetExtension(filePath).ToLower() == ".sdlxliff")
                {
                    throw new NotSupportedException("MemoQ is not supported");
                    /*var mqXliffPath = Path.Combine(jobDataFolder, "mq.xlf");
                    if (!File.Exists(mqXliffPath) || !File.Exists(mqXliffPath.Replace(".xlf", ".mqxlz")))
                    {
                        //create the mq xliff file
                        MemoQHelper.CreateXliffFromDocument((int)DATA.DataTypes.CATServer.Pollux, idTranslation.ToString(), sTranslationDir,
                            Path.GetFileName(mqXliffPath), sFilePath, sFilterPath, null, idFrom, idTo, false, -1);
                    }

                    fileContent = File.ReadAllBytes(mqXliffPath);
                    var sTmpFilterPath = GetDefaultFilter(mqXliffPath); //in case if we will have defult filter for xliff
                    var sTmpFiltername = sTmpFilterPath != null ? Path.GetFileName(sTmpFilterPath) : null;
                    var tmpFilterContent = sTmpFilterPath != null ? File.ReadAllBytes(sTmpFilterPath) : null;
                    //assemble the mq xliff file
                    aOutFileBytes = client.CreateDocumentFromXliff(Path.GetFileName(mqXliffPath), fileContent, sTmpFilterPath,
                        tmpFilterContent, sLangFrom, sLangTo, xlfFile.OuterXml);
                    var mqXliffContent = System.Text.Encoding.UTF8.GetString(aOutFileBytes);
                    mqXliffContent = mqXliffContent.Replace("mq:status=\"NotStarted\"", "mq:status=\"ManuallyConfirmed\"");
                    //assemble the final document
                    String sTempDir = ConfigurationSettings.AppSettings["TempFolder"].ToString() + Guid.NewGuid();
                    cZipHelper.UnZipFiles(mqXliffPath.Replace(".xlf", ".mqxlz"), sTempDir, null);
                    File.WriteAllText(Path.Combine(sTempDir, "document.mqxliff"), mqXliffContent);
                    var oZh = new cZipHelper();
                    String mqxlzFile = Path.Combine(sTranslationDir, "out.mqxlz");
                    oZh.ZipFiles(new String[] { Path.Combine(sTempDir, "document.mqxliff"), Path.Combine(sTempDir, "skeleton.xml") },
                        mqxlzFile, null, false);
                    //create the document               
                    String sTmpFilePath = MemoQHelper.CreateDocumentFromXliff((int)DATA.DataTypes.CATServer.Cronos, mqxlzFile, idTranslation, idFrom,
                        idTo, sTranslationDir);
                    Directory.Delete(sTempDir, true);
                    File.Delete(mqxlzFile);
                    aOutFileBytes = File.ReadAllBytes(sTmpFilePath);
                    File.Delete(sTmpFilePath);*/
                }
                else
                {
                    if (fileExtension == ".mqxlz")
                    {
                        String sTempDir = _configuration!["TempFolder"]! + Guid.NewGuid();
                        ZipHelper.UnZipFiles(filePath, sTempDir, null);
                        var tmFilePath = Path.Combine(sTempDir, "document.mqxliff");
                        sFilename = Path.GetFileName(tmFilePath);
                        fileContent = File.ReadAllBytes(tmFilePath);
                        aOutFileBytes = client.CreateDocumentFromXliff(sFilename, fileContent, sFiltername,
                            filterContent, sourceLanguage, targetLanguage, xlfFile.OuterXml);
                        var mqXliffContent = System.Text.Encoding.UTF8.GetString(aOutFileBytes);
                        mqXliffContent = mqXliffContent.Replace("mq:status=\"NotStarted\"", "mq:status=\"ManuallyConfirmed\"");
                        File.WriteAllText(Path.Combine(sTempDir, "document.mqxliff"), mqXliffContent);
                        var zipHelper = new ZipHelper();
                        String mqxlzFile = Path.Combine(jobDataFolder, "out.mqxlz");
                        zipHelper.ZipFiles(new String[] { Path.Combine(sTempDir, "document.mqxliff"), Path.Combine(sTempDir, "skeleton.xml") },
                            mqxlzFile, false);
                        aOutFileBytes = File.ReadAllBytes(mqxlzFile);
                        File.Delete(mqxlzFile);
                        Directory.Delete(sTempDir, true);
                    }
                    else
                    {
                        aOutFileBytes = client.CreateDocumentFromXliff(sFilename, fileContent, sFiltername,
                            filterContent, sourceLanguage, targetLanguage, xlfFile.OuterXml);
                    }
                }

                /*tmpFilePath = Path.Combine(_configuration!["TempFolder"]!, Guid.NewGuid().ToString());
                //save the output file
                File.WriteAllBytes(tmpFilePath, aOutFileBytes);
                lstFilesToDelete.Add(tmpFilePath);

                //post-process document
                sOutFilePath = DocumentProcessor.PostProcessDocument(idJob, tmpFilePath);*/

                var fileData = new FileData()
                {
                    Content = aOutFileBytes,
                    FileName = "tmp" + idJob.ToString() + fileExtension
                };

                return fileData;
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateDoc Error -> idJob: " + idJob + " " + ex.Message);
                throw;
            }
            finally
            {
                foreach (String sPath in lstFilesToDelete)
                    File.Delete(sPath);
            }
        }

        private TMAssignment[] GetTMAssignments(int idCorporateProfile, string sourceLanguage, string[] targetLanguages, int speciality, bool createTM)
        {
            return null!;
        }

    }
}
