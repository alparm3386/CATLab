using CAT_web.Data;
using CAT_web.Helpers;
using CAT_web.Models;
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
using CAT_web.Enums;

namespace CAT_web.Services.CAT
{
    public class CATClientService
    {
        private readonly CAT_webContext _context;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// CATClientService
        /// </summary>
        public CATClientService(CAT_webContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
               || sExt == ".json" || sExt == ".idml" || sExt == ".sdlppx"
               || (ConfigurationSettings.AppSettings["PdfSupport"] == "true" && sExt == ".pdf"))
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

        public Statistics[] GetStatisticsForDocument(string sFilePath, string sFilterPath, String sourceLang,
            string[] aTargetLangs)
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
                    sFilePath = CATUtils.ExtractMQXlz(sFilePath);
                    lstFilesToDelete.Add(sFilePath);
                }

                //set the parameters
                String sFilename = Path.GetFileName(sFilePath);
                byte[] fileContent = File.ReadAllBytes(sFilePath);
                String sFiltername = Path.GetFileName(sFilterPath);
                byte[] filterContent = null;
                if (File.Exists(sFilterPath))
                    filterContent = File.ReadAllBytes(sFilterPath);
                TMAssignment[] aTMs = new TMAssignment[] { new TMAssignment() { tmPath = "29610/__35462_en_fr" } };

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

                String sFiltername = File.Exists(sFilterPath) ? Path.GetFileName(sFilterPath) : null;
                byte[] filterContent = sFiltername != null ? File.ReadAllBytes(sFilterPath) : null;
                TMAssignment[] aTMs = new TMAssignment[] { new TMAssignment() { tmPath = "29610/__35462_en_fr" } };
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
                    if (job?.DateProcessed is null)
                        throw new Exception("Already processed.");

                    //Get the document
                    var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]);
                    string filePath = Path.Combine(sourceFilesFolder, job.FileName!);
                    var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]);

                    //get the filter
                    string filterPath = "";
                    if (job.FilterName != null)
                        filterPath = Path.Combine(sourceFilesFolder, job.FilterName);

                    var jobDataFolder = CATUtils.GetJobDataFolder(idJob);

                    //var aContentForMT = new List<MTContent>();

                    String sXlifFilePath = CATUtils.CreateXlfFilePath(idJob, DocumentType.original); //we do a backup of the original xliff

                    //pre-process the document
                    String tmpFilePath = null;// DocumentProcessor.PreProcessDocument(sFilePath, idFilter, idFrom, idTo);
                    if (tmpFilePath != null)
                    {
                        filePath = tmpFilePath;
                        filesToDelete.Add(tmpFilePath);
                    }

                    if (CATUtils.IsCompressedMemoQXliff(filePath))
                    {
                        filePath = CATUtils.ExtractMQXlz(filePath);
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
                        var translationUnit = new TranslationUnit();
                        translationUnit.tuid = nIdx;
                        var tuId = tu.Attributes["id"].Value;
                        var targetNode = tu["target"];
                        XmlNodeList sourceSegments = null;
                        var ssNode = tu["seg-source"];

                        if (ssNode == null)
                            sourceSegments = tu.SelectNodes("x:source", xmlnsManager);
                        else
                            sourceSegments = ssNode.SelectNodes("x:mrk", xmlnsManager);

                        foreach (XmlNode sourceSegment in sourceSegments)
                        {
                            if (CATUtils.IsSegmentEmptyOrWhiteSpaceOnly(sourceSegment.InnerXml.Trim()))
                                continue;

                            var mid = "-1";
                            if (sourceSegment.Name == "mrk")
                                mid = sourceSegment.Attributes["mid"].Value;
                            var source = CATUtils.XliffTags2TMXTags(sourceSegment.InnerXml.Trim());

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

                            translationUnit.sourceText = source;

                            XmlNode targetSegment = null;
                            if (sourceSegment.Name == "mrk")
                                targetSegment = targetNode.SelectSingleNode("x:mrk[@mid=" + mid + "]", xmlnsManager);
                            else
                                targetSegment = targetNode;
                            if (matchQuality >= 100 || bEdited)
                            {
                                String sTarget = CATUtils.XmlTags2GoogleTags(targetSegment.InnerXml, CATUtils.TagType.Xliff); //we store the target text with google tags
                                translationUnit.targetText = sTarget;
                            }
                            nIdx++;
                        }
                    }

                    //machine translation
                    var mTranslator = MTranslator.Create(MTProvider);
                    var translations = mTranslator.Translate(aContentForMT, sLangFrom639_1, sLangTo639_1, null);
                    //var risks = new Dictionary

                    cDocumentsManager oDMgr = new cDocumentsManager(oTMgr);
                    //start a new transaction for the MT
                    TUTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
                    var MTLookup = new HashSet<String>();
                    using (TUTransaction)
                    {
                        foreach (var oTranslation in translations)
                        {
                            if (bPreTranslateWithMT)
                                oDMgr.DBfactory.InsertTranslatedSection(oTranslation.idSourceContent, oTranslation.target, false, 0);

                            var tmpIds = oTranslation.idXliff.Split('@');
                            XmlNode tuNode = Xliff.SelectSingleNode("//x:trans-unit[@id='" + tmpIds[0] + "']", xmlnsManager);
                            var sourceSegment = !String.IsNullOrEmpty(tmpIds[1]) ? tuNode.SelectSingleNode("x:seg-source/x:mrk[@mid=" + tmpIds[1] + "]", xmlnsManager) : null;
                            if (sourceSegment == null)
                                sourceSegment = tuNode.SelectSingleNode("x:source", xmlnsManager);
                            var status = sourceSegment.Attributes["status"];
                            if (status == null)
                            {
                                status = Xliff.CreateAttribute("status");
                                sourceSegment.Attributes.Append(status);
                            }

                            status.Value = "tmMachineTranslated";
                            var tagsMap = CATUtils.GetTagsMap(sourceSegment.InnerXml, CATUtils.TagType.Xliff);
                            var targetSegment = !String.IsNullOrEmpty(tmpIds[1]) ? tuNode.SelectSingleNode("x:target/x:mrk[@mid=" + tmpIds[1] + "]", xmlnsManager) : null;
                            if (targetSegment == null)
                                targetSegment = tuNode.SelectSingleNode("x:target", xmlnsManager);
                            targetSegment.InnerXml = CATUtils.GoogleTags2XmlTags(oTranslation.target, tagsMap);

                            if (!MTLookup.Contains(oTranslation.source))
                            {
                                var hash = Utils.GenerateHash(oTranslation.source);
                                oDMgr.DBfactory.InsertMT(idTranslation, (int)MTProvider, oTranslation.target, hash,
                                    oTranslation.source, sLangFrom639_1 + sLangTo639_1, oTranslation.risk);
                            }
                            else
                                MTLookup.Add(oTranslation.source);
                        }

                        //save the machine translation into the original xliff
                        Xliff.Save(sXlifFilePath);

                    }
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

    }
}
