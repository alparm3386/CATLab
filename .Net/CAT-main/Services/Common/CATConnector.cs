using System.Data;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Xml;
using CAT.Enums;
using CAT.Services.MT;
using CAT.Models.Common;
using Statistics = CAT.Models.Common.Statistics;
using TMAssignment = CAT.Models.Common.TMAssignment;
using AutoMapper;
using CAT.Data;
using CAT.Helpers;
using Microsoft.EntityFrameworkCore;
using CAT.Models.Entities.TranslationUnits;
using Grpc.Net.Client;
using static Proto.CAT;
using Google.Protobuf;
using Proto;
using System.Transactions;
using TMType = CAT.Enums.TMType;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Protocols.WsTrust;
using Newtonsoft.Json;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public class CATConnector : ICATConnector
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<IMachineTranslator> _machineTranslators;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IDocumentProcessor _documentProcessor;
        private readonly ILanguageService _languageService;
        private readonly CatClientFactory _catClientFactory;

        //private static int MATCH_THRESHOLD = 50;

        /// <summary>
        /// CATClientService
        /// </summary>
        public CATConnector(DbContextContainer dbContextContainer, IConfiguration configuration, IEnumerable<IMachineTranslator> machineTranslators,
            ILanguageService languageService, CatClientFactory catClientFactory, IMapper mapper, ILogger<CATConnector> logger, 
            IDocumentProcessor documentProcessor)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _machineTranslators = machineTranslators;
            _mapper = mapper;
            _logger = logger;
            _documentProcessor = documentProcessor;
            _languageService = languageService;
            _catClientFactory = catClientFactory;
        }

        private CATClient GetCatClient()
        {
            return _catClientFactory.CreateClient();
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

        public async Task<Statistics[]> GetStatisticsForDocument(string sFilePath, string sFilterPath, int sourceLang,
            int[] aTargetLangs, TMAssignment[] tmAssignments)
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

                var aTMs = Array.ConvertAll(tmAssignments,
                    tma => new Proto.TMAssignment() { Penalty = tma.penalty, Speciality = tma.speciality, TmId = tma.tmId });

                //the target language array
                var lstTargetLangs = new List<String>();
                var aRet = new List<Statistics>();
                foreach (int targetLang in aTargetLangs)
                {
                    var targetLanguageIso639_1 = await _languageService.GetLanguageCodeIso639_1(targetLang);
                    var sourceLanguageIso639_1 = await _languageService.GetLanguageCodeIso639_1(sourceLang);
                    var catClient = GetCatClient();
                    var request = new GetStatisticsForDocumentRequest
                    {
                        FileName = sFilename,
                        FileContent = ByteString.CopyFrom(fileContent),
                        FilterName = sFiltername ?? "",
                        FilterContent = filterContent != null ? ByteString.CopyFrom(filterContent) : ByteString.Empty,
                        SourceLangISO6391 = sourceLanguageIso639_1,
                        TargetLangsISO6391 = { targetLanguageIso639_1 },
                        TMAssignments = { aTMs }
                    };

                    var stats = catClient.GetStatisticsForDocument(request).Statistics;
                    aRet.Add(new Statistics()
                    {
                        repetitions = stats[0].Repetitions,
                        match_101 = stats[0].Match101,
                        match_100 = stats[0].Match100,
                        match_95_99 = stats[0].Match9599,
                        match_85_94 = stats[0].Match8594,
                        match_75_84 = stats[0].Match7584,
                        match_50_74 = stats[0].Match5074,
                        no_match = stats[0].NoMatch,
                        sourceLang = sourceLang,
                        targetLang = targetLang
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
            string targetLang, int iGoodMatchRate, TMAssignment[] tmAssignments)
        {
            try
            {
                //check the filter
                if (String.IsNullOrEmpty(sFilterPath))
                    sFilterPath = GetDefaultFilter(sFilePath);

                var matchThreshold = (iGoodMatchRate >= 50 && iGoodMatchRate <= 101) ? iGoodMatchRate : 99;
                //set the parameters
                String sFilename = Path.GetFileName(sFilePath);
                byte[] fileContent = File.ReadAllBytes(sFilePath);

                String? sFiltername = File.Exists(sFilterPath) ? Path.GetFileName(sFilterPath) : null;
                byte[]? filterContent = sFiltername != null ? File.ReadAllBytes(sFilterPath) : null;
                var aTMs = Array.ConvertAll(tmAssignments,
                    tma => new Proto.TMAssignment() { Penalty = tma.penalty, Speciality = tma.speciality, TmId = tma.tmId });
                var catClient = GetCatClient();
                var request = new Proto.CreateXliffFromDocumentRequest
                {
                    FileName = sFilename,
                    FileContent = ByteString.CopyFrom(fileContent),
                    FilterName = sFiltername ?? "",
                    FilterContent = filterContent != null ? ByteString.CopyFrom(filterContent) : ByteString.Empty,
                    SourceLangISO6391 = sourceLang,
                    TargetLangISO6391 = targetLang,
                    TmAssignments = { aTMs }
                };

                String sXliffContent = catClient.CreateXliffFromDocument(request).XliffContent;

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

        public void ParseDoc(int jobId)
        {
            if (IsLocked(String.Intern(GetParseDocLockString(jobId))))
                throw new Exception("Translation is locked: " + jobId.ToString());

            int nIdx = 1;
            List<String> filesToDelete = new List<String>();
            int iThreshold = 100;

            try
            {
                lock (String.Intern(GetParseDocLockString(jobId))) //lock on the job id
                {
                    //get the translation details
                    var job = _dbContextContainer.MainContext.Jobs.Include(j => j.Quote)
                        .Include(j => j.Order).ThenInclude(c => c!.Client).FirstOrDefault(j => j.Id == jobId);
                    //document
                    var document = _dbContextContainer.MainContext.Documents.Find(job!.SourceDocumentId);

                    //check the translation units
                    var tuNum = _dbContextContainer.TranslationUnitsContext.TranslationUnit
                                     .Where(tu => tu.documentId == document.Id).OrderBy(tu => tu.tuid).Count();
                    if (tuNum > 0)
                        return;

                    //Get the document
                    var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]!);
                    string filePath = Path.Combine(sourceFilesFolder, document!.FileName!);

                    //get the filter
                    string? filterPath = null;
                    var docfFilter = _dbContextContainer.MainContext.DocumentFilters.FirstOrDefault(docFilter => docFilter.DocumentId == document.Id);
                    if (docfFilter != null)
                    {
                        var filter = _dbContextContainer.MainContext.Filters.FirstOrDefault(filter => filter.Id == docfFilter.FilterId)!;
                        var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]!);
                        filterPath = Path.Combine(fileFiltersFolder, filter.FilterName!);
                    }

                    var jobDataFolder = CATUtils.GetJobDataFolder(jobId, _configuration["JobDataBaseFolder"]!);

                    //var aContentForMT = new List<MTContent>();

                    String sXlifFilePath = CATUtils.CreateXlfFilePath(jobId, DocumentType.Original, _configuration["JobDataBaseFolder"]!, true); //we do a backup of the original xliff

                    //pre-process the document
                    String tmpFilePath = _documentProcessor.PreProcessDocument(filePath, filterPath!);
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
                    var aTMAssignments = GetTMAssignments(job!.Order!.Client.CompanyId, job!.Quote!.SourceLanguage,
                        new int[] { job!.Quote!.TargetLanguage }, job.Quote.Speciality, true);

                    CreateXliffFromDocument(jobDataFolder, Path.GetFileName(sXlifFilePath), filePath, filterPath!,
                        _languageService.GetLanguageCodeIso639_1(job!.Quote!.SourceLanguage!).Result,
                        _languageService.GetLanguageCodeIso639_1(job!.Quote!.TargetLanguage!).Result, iThreshold, aTMAssignments);

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
                            translationUnit.documentId = document.Id;

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
                    mmt.Translate(translatables, _languageService.GetLanguageCodeIso639_1(job!.Quote!.SourceLanguage!).Result,
                        _languageService.GetLanguageCodeIso639_1(job!.Quote!.TargetLanguage!).Result, null);

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
                var jobDataFolder = CATUtils.GetJobDataFolder(idJob, _configuration["JobDataBaseFolder"]!);
                int iThreshold = 100;

                //get the job details
                var job = _dbContextContainer.MainContext.Jobs.Include(j => j.Quote).FirstOrDefault(j => j.Id == idJob);

                //get the document
                var document = _dbContextContainer.MainContext.Documents.Find(job!.SourceDocumentId);
                var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]!);
                string filePath = Path.Combine(sourceFilesFolder, document!.FileName!);

                //get the filter
                string? filterPath = null;
                var docfFilter = _dbContextContainer.MainContext.DocumentFilters.FirstOrDefault(docFilter => docFilter.DocumentId == document.Id);
                if (docfFilter != null)
                {
                    var filter = _dbContextContainer.MainContext.Filters.FirstOrDefault(filter => filter.Id == docfFilter.FilterId)!;
                    var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]!);
                    filterPath = Path.Combine(fileFiltersFolder, filter.FilterName!);
                }

                if (String.IsNullOrEmpty(filterPath))
                    filterPath = GetDefaultFilter(filePath);


                //pre-process the document
                String tmpFilePath = _documentProcessor.PreProcessDocument(filePath, filterPath!);
                if (tmpFilePath != null)
                {
                    filePath = tmpFilePath;
                    lstFilesToDelete.Add(tmpFilePath);
                }

                var sourceLanguage = job!.Quote!.SourceLanguage!;
                var targetLanguage = job!.Quote!.TargetLanguage!;
                var sourceLangIso639_1 = _languageService.GetLanguageCodeIso639_1(sourceLanguage).Result;
                var targetLangIso639_1 = _languageService.GetLanguageCodeIso639_1(targetLanguage).Result;
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
                        sourceLangIso639_1, targetLangIso639_1, iThreshold, null!);
                }

                //get the translated texts
                var translationUnits = _dbContextContainer.TranslationUnitsContext.TranslationUnit
                                 .Where(tu => tu.documentId == document.Id).OrderBy(tu => tu.tuid).ToList();

                //fill the xliff file with the translations
                XmlDocument xlfFile = new XmlDocument();
                xlfFile.PreserveWhitespace = true;
                xlfFile.Load(xlifFilePath);
                var xmlnsManager = new XmlNamespaceManager(xlfFile.NameTable);
                xmlnsManager.AddNamespace("x", xlfFile.DocumentElement!.NamespaceURI);
                var transUnits = xlfFile.SelectNodes("//x:trans-unit", xmlnsManager);
                var tmEntries = new List<TMEntry>();
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
                        var tmEntry = new Proto.TMEntry();
                        tmEntry.Source = CATUtils.XliffTags2TMXTags(sourceSegment.InnerXml);
                        tmEntry.Target = CATUtils.XliffTags2TMXTags(sTranslatedText);

                        tmEntries.Add(tmEntry);
                        tuid++;
                    }
                }

                if (translationUnits.Count != tuid - 1)
                    throw new Exception("Segments don't match.");


                //update the TMs
                var catClient = GetCatClient();
                if (updateTM)
                {
                    //prepare the TM entries
                    for (int i = 0; i < tmEntries.Count; i++)
                    {
                        var metadata = new Dictionary<String, String>();
                        if (userId != null)
                            metadata.Add("user", userId.ToString());
                        metadata.Add("speciality", job.Quote.Speciality.ToString());
                        metadata.Add("jobId", job.Id.ToString());

                        //preceding segment
                        if (i > 0)
                            metadata.Add("prevSegment", tmEntries[i - 1].Source);
                        //following segment 
                        if (i < tmEntries.Count - 1)
                            metadata.Add("nextSegment", tmEntries[i + 1].Source);

                        tmEntries[i].Metadata = JsonConvert.SerializeObject(metadata);
                    }

                    //update the TMs
                    var TMs = GetTMAssignments(1044, job!.Quote!.SourceLanguage, new int[] { job!.Quote!.TargetLanguage! },
                        job!.Quote!.Speciality, true);
                    foreach (var tm in TMs)
                    {
                        if (!tm.isReadonly)
                        {
                            var request = new Proto.AddTMEntriesRequest
                            {
                                TMEntries = { tmEntries },
                                TmId = tm.tmId
                            };
                            catClient.AddTMEntries(request);
                        }
                    }
                }

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
                String fileName = Path.GetFileName(filePath);
                byte[] fileContent = File.ReadAllBytes(filePath);
                String? filterName = null;
                byte[]? filterContent = null;

                if (!String.IsNullOrEmpty(filterPath))
                {
                    filterName = Path.GetFileName(filterPath);
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
                        fileName = Path.GetFileName(tmFilePath);
                        fileContent = File.ReadAllBytes(tmFilePath);
                        aOutFileBytes = null;// client.CreateDocumentFromXliff(sFilename, fileContent, sFiltername,
                                             //filterContent, _languageService.GetLanguageCodeIso639_1(sourceLanguage).Result,
                                             //_languageService.GetLanguageCodeIso639_1(targetLanguage).Result, xlfFile.OuterXml);
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
                        var request = new CreateDocumentFromXliffRequest()
                        {
                            FileName = fileName,
                            FileContent = ByteString.CopyFrom(File.ReadAllBytes(filePath)),
                            FilterName = filterName ?? "",
                            FilterContent = filterContent != null ? ByteString.CopyFrom(filterContent) : ByteString.Empty,
                            SourceLangISO6391 = sourceLangIso639_1,
                            TargetLangISO6391 = targetLangIso639_1,
                            XliffContent = xlfFile.OuterXml
                        };

                        aOutFileBytes = catClient.CreateDocumentFromXliff(request).Document.ToByteArray();
                    }
                }
                //post-process document
                //sOutFilePath = DocumentProcessor.PostProcessDocument(idJob, tmpFilePath);

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

        public TMAssignment[] GetTMAssignments(int companyId, int sourceLang, int[] targetLangs, int speciality, bool createTM)
        {
            var tmAssignments = new List<TMAssignment>();
            //only company TM
            var catClient = GetCatClient();
            foreach (var targetLang in targetLangs)
            {
                var tmId = CreateTMId(companyId, companyId, sourceLang, targetLang, TMType.CompanyPrimary);
                var tmExistsRequest = new TMExistsRequest { TmId = tmId };
                var exists = catClient.TMExists(tmExistsRequest).Exists;
                if (!exists && createTM)
                {
                    var createTMRequest = new CreateTMRequest() { TmId = tmId };
                    catClient.CreateTM(createTMRequest);
                    exists = true;
                }

                if (exists)
                {
                    var tmAssignment = new TMAssignment()
                    {
                        isGlobal = false,
                        isReadonly = false,
                        penalty = 0,
                        speciality = speciality,
                        tmId = tmId
                    };
                    tmAssignments.Add(tmAssignment);
                };
            }

            return tmAssignments.ToArray();
        }

        private String CreateTMId(int groupId, int companyId, int sourceLang, int targetLang, TMType type)
        {
            var sourceLangIso639_1 = _languageService.GetLanguageCodeIso639_1(sourceLang).Result;
            var targetLangIso639_1 = _languageService.GetLanguageCodeIso639_1(targetLang).Result;
            var tmPrefix = "";
            if (type == TMType.Global)
                tmPrefix = "$_";
            else if (type == TMType.GroupPrimary)
                tmPrefix = "_";
            else if (type == TMType.GroupSecondary)
                tmPrefix = "_sec_";
            else if (type == TMType.CompanyPrimary)
                tmPrefix = "__";
            else if (type == TMType.CompanySecondary)
                tmPrefix = "__sec_";

            return groupId + "/" + tmPrefix + companyId + "_" + sourceLangIso639_1 + "_" + targetLangIso639_1;
        }
    }
}
