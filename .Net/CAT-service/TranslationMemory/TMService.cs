using cat.service;
using cat.utils;
using CAT.Models;
using CATService.Backup;
using CATService.Utils;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Newtonsoft.Json;
using okapi;
using okapi.resource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;
using System.Xml;
using utils;

namespace CAT.tm
{
    class TMService
    {
        private static readonly Object TMLock = new Object();
        private static readonly int TmWriterIdleTimeout = 20; //minutes
        private static readonly int MaxConnectionPoolSize = 40;
        private static Dictionary<String, TMConnector> TmConnectionPool = new Dictionary<String, TMConnector>();
        private static readonly String RepositoryFolder = System.Configuration.ConfigurationSettings.AppSettings["TMPath"];
        private OkapiConnector okapiConnector = new OkapiConnector();
        private static readonly int NGramLength = 4;
        private static Dictionary<int, String> specialities = new Dictionary<int, String>();
        private readonly IConfiguration _configuration;
        private readonly ICATConnector _catConnector;
        private readonly ILogger _logger;

        static TMService()
        {
            // Initialization code for static members

            //load the specialities only one time
            var dbFactory = new DBFactory();
            var dsSpecialities = dbFactory.GetSpecialities();
            foreach (DataRow row in dsSpecialities.Tables[0].Rows)
            {
                specialities.Add((int)row["specialityID"], row["description"].ToString().ToLower());
            }
        }

        public TMService(IConfiguration configuration, ILogger<TMService> logger, ICATConnector catConnector)
        {
            _configuration = configuration;
            _logger = logger;
            _catConnector = catConnector;
        }

        private static String GetSourceIndexDirectory(String tmPath)
        {
            var tmName = tmPath.Split('/')[1];
            var tmDir = GetTMDirectory(tmPath);
            var sourceIndexDir = Path.Combine(tmDir, "source indexes" + "/" + tmName);

            return sourceIndexDir;
        }

        private static String GetTMName(String tmPath)
        {
            var tmName = tmPath.Split('/')[1];
            return tmName;
        }

        private static String GetTMDirectory(String tmPath)
        {
            var parent = tmPath.Split('/')[0];
            var tmDir = Path.Combine(REPOSITORY_FOLDER, parent);

            return tmDir;
        }

        private static String GetDatabaseName(String tmPath)
        {
            return tmPath.Split('/')[0];
        }

        /// <summary>
        /// CreateTM
        /// </summary>
        /// <param name="tmPath"></param>
        /// <returns></returns>
        public void CreateTM(String tmPath)
        {
            try
            {
                //check if the TM Exists
                if (TMExists(tmPath))
                    return;

                //check the TM directory
                var tmDir = GetTMDirectory(tmPath);
                if (!System.IO.Directory.Exists(tmDir))
                    System.IO.Directory.CreateDirectory(tmDir);

                //create the SQL repository
                dbFactory.CreateTranslationMemory(tmPath);

                //create the lucene repository
                var sourceIndexDirectory = GetSourceIndexDirectory(tmPath);
                if (!System.IO.Directory.Exists(sourceIndexDirectory))
                {
                    System.IO.Directory.CreateDirectory(sourceIndexDirectory);
                    var tmWriter = TmWriterFactory.CreateFileBasedTmWriter(sourceIndexDirectory, true);
                    tmWriter.Commit();
                    tmWriter.Close();
                }

                //backup
                backupClient.CreateTM(tmPath);
            }
            catch (Exception ex)
            {
                logger.Log("TMErrors.log", "ERROR: CreateTM -> TM name: " + tmPath + "\n" + ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// TMExists
        /// </summary>
        /// <param name="sTMName"></param>
        /// <returns></returns>
        public bool TMExists(String tmPath)
        {
            //the TM directory
            var tmDir = GetTMDirectory(tmPath);
            if (!System.IO.Directory.Exists(tmDir))
                return false;

            //the TM table
            if (!dbFactory.TMExists(tmPath))
                return false;

            //the source index directory
            var sourceIndexDirectory = GetSourceIndexDirectory(tmPath);
            if (!System.IO.Directory.Exists(sourceIndexDirectory))
                return false;

            return true;
        }

        /// <summary>
        /// GetStatisticsForDocument
        /// </summary>
        /// <param name="sFileName"></param>
        /// <param name="fileContent"></param>
        /// <param name="sFilterName"></param>
        /// <param name="filterContent"></param>
        /// <param name="sourceLangISO639_1"></param>
        /// <param name="aTargetLangsISO639_1"></param>
        /// <param name="aTMAssignments"></param>
        /// <returns></returns>
        public Statistics[] GetStatisticsForDocument(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent, String sourceLangISO639_1,
        String[] aTargetLangsISO639_1, TMAssignment[] aTMAssignments)
        {
            long lStart = CATUtils.CurrentTimeMillis();
            //convert the document to xliff
            var sXliffContent = okapiConnector.CreateXliffFromDocument(sFileName, fileContent, sFilterName, filterContent, sourceLangISO639_1, "fr"); //we can use a dummy language here

            //get the text fragments
            var tmpXliff = new XmlDocument();
            tmpXliff.LoadXml(sXliffContent);
            XmlNamespaceManager xmlnsmgr = new XmlNamespaceManager(tmpXliff.NameTable);
            xmlnsmgr.AddNamespace("x", tmpXliff.DocumentElement.NamespaceURI);

            var lstTranslationUnits = new List<TranslationUnit>();
            var tus = tmpXliff.GetElementsByTagName("trans-unit");
            for (int i = 0; i < tus.Count; i++)
            {
                var tu = (XmlElement)tus[i]; // check the segmentation
                if (tu.Attributes["translate"]?.Value.ToLower() == "no")
                    continue; //skip non-translatables
                XmlNodeList segmentNodes = null;
                var ssNode = tu["seg-source"];
                if (ssNode == null)
                    segmentNodes = tu.SelectNodes("x:source", xmlnsmgr);
                else
                    segmentNodes = ssNode.SelectNodes("x:mrk", xmlnsmgr);
                for (int j = 0; j < segmentNodes.Count; j++)
                {
                    var segmentNode = (XmlElement)segmentNodes[j]; // the source segment
                    if (segmentNode.Attributes["translate"]?.Value.ToLower() == "no")
                        continue; //skip non-translatables

                    var sourceText = segmentNode.InnerXml.Trim();
                    if (String.IsNullOrEmpty(sourceText))
                        continue;
                    var source = CATUtils.XliffSegmentToTextFragmentSimple(sourceText);
                    var transUnit = new TranslationUnit(source, null, null);
                    lstTranslationUnits.Add(transUnit);
                }
            }

            //set the contexts
            for (int i = 0; i < lstTranslationUnits.Count; i++)
            {
                var tu = lstTranslationUnits[i];
                String prev = "";
                String next = "";
                if (i != 0)
                    prev = lstTranslationUnits[i - 1].source.GetCodedText();
                if (i < lstTranslationUnits.Count - 1)
                    next = lstTranslationUnits[i + 1].source.GetCodedText();
                //the context
                lstTranslationUnits[i].context = CATUtils.djb2hash(prev + next).ToString();
            }

            var lstStats = new List<Statistics>();
            foreach (var sTargetLang in aTargetLangsISO639_1)
            {
                //TMs for the current language pair
                TMAssignment[] currentTMAssignments = null;
                //check the languages for the TMs
                if (aTMAssignments != null)
                {
                    var lstTmpTMs = new List<TMAssignment>();
                    foreach (var tmAssignment in aTMAssignments)
                    {
                        var tmInfo = GetTMInfo(tmAssignment.tmPath, false);
                        if (tmInfo.langFrom.ToLower() == sourceLangISO639_1.ToLower() && tmInfo.langTo.ToLower() == sTargetLang.ToLower())
                            lstTmpTMs.Add(tmAssignment);
                    }
                    currentTMAssignments = lstTmpTMs.ToArray();
                }
                else
                    aTMAssignments = new TMAssignment[0];

                //the statistics
                var stat = GetStatisticsForTranslationUnits(lstTranslationUnits, sourceLangISO639_1, currentTMAssignments, true);
                stat.targetLang = sTargetLang;
                lstStats.Add(stat);
            }

            long lElapsed = CATUtils.CurrentTimeMillis() - lStart;
            logger.Log("Statistics.log", $"{sFileName}  -> {lElapsed}ms");
            //logger.Log("Benchmark.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\tGetStatisticsForDocument\t" + lElapsed 
            //    + "\t" + sFileName + "\t" + sourceLangISO639_1 + "_" + String.Join(",", aTargetLangsISO639_1) + "_" + 
            //    String.Join(",", Array.ConvertAll(aTMAssignments, o => o.tmPath)));

            return lstStats.ToArray();
        }

        /// <summary>
        /// GetStatisticsForTranslationUnits
        /// </summary>
        /// <param name="textFragments"></param>
        /// <param name="aTMAssignments"></param>
        /// <param name="bWithHomogeneity"></param>
        /// <returns></returns>
        private Statistics GetStatisticsForTranslationUnits(List<TranslationUnit> transUnits, String sourceLangISO639_1,
            TMAssignment[] aTMAssignments, bool bWithHomogeneity)
        {
            try
            {
                long lStart = CATUtils.CurrentTimeMillis();
                //the word counter
                var wordCounter = new SimpleWordcount(sourceLangISO639_1);
                var maxDocs = 0;
                var scoreCntr = 0;
                var loopCntr1 = 0;
                var CUT_OFF_RATIO = 1;
                var ROUGH_CUTOFF = 0.5;
                String searchFieldName = "SOURCE";
                // inverted index on source
                List<List<String>> textFragmentsUniqueTermsList = new List<List<String>>();
                var scores = new List<Score>();
                var homogeneityIndex = new InvertedIndex();
                var repetitionsTable = new Dictionary<String, int>();
                //data table for the incontext matches
                DataTable dtQuery = new DataTable();
                dtQuery.Columns.Add("id");
                dtQuery.Columns.Add("source");
                dtQuery.Columns.Add("sourceHash");
                dtQuery.Columns.Add("context");
                for (int i = 0; i < transUnits.Count; i++)
                {
                    var tu = transUnits[i];
                    var textFragment = tu.source;
                    //the unique terms lists
                    var text = textFragment.GetText();
                    var terms = homogeneityIndex.GetUniqueTerms(text);
                    textFragmentsUniqueTermsList.Add(terms);

                    // check repetitions
                    var codedText = textFragment.GetCodedText();
                    var idx = 0;
                    repetitionsTable.TryGetValue(codedText, out idx);
                    //the incontext query
                    dtQuery.Rows.Add(i, codedText, CATUtils.djb2hash(codedText), tu.context);
                    if (idx > 0)
                    {
                        scores.Add(scores[idx - 1].cloneAsRepetition());
                        continue;
                    }
                    else
                        repetitionsTable.Add(codedText, i + 1); //we cannot use zero based index

                    // homogeneity check
                    float highestScore = 0;
                    if (bWithHomogeneity)
                        highestScore = homogeneityIndex.GetHighestScore(text, 50);
                    // the word count
                    var words = wordCounter.CountWords(text);//CountWords(textFragment);
                    var score = new Score(i, (int)words, highestScore, false);
                    scores.Add(score);
                    homogeneityIndex.AddToIndex(text);
                }

                Debug.WriteLine("homogeneity: " + (CATUtils.CurrentTimeMillis() - lStart) + "ms.");

                var uniqueTerms = homogeneityIndex.GetUniqueTermsFromIndex();

                //get the statistics
                var stats = new Statistics();
                if (aTMAssignments != null)
                {
                    foreach (var tmAssignment in aTMAssignments)
                    {
                        var bUseSpeciality = tmAssignment.speciality >= 0;
                        String speciality = "," + tmAssignment.speciality.ToString() + ","; //not too nice but ok
                        //get the incontext matches in a background thread
                        Thread contextCheckThread = null;
                        DataSet dsContexts = null;
                        if (tmAssignment.penalty <= 2)
                        {
                            contextCheckThread = new Thread(() =>
                            {
                                try
                                {
                                    dsContexts = dbFactory.CheckIncontextMatches(tmAssignment.tmPath, dtQuery);
                                }
                                catch (Exception ex)
                                {
                                    logger.Log("Statistics.log", "ERROR: CheckIncontextMatches -> " + ex.ToString());
                                }
                            });
                            contextCheckThread.Start();
                        }

                        //String tmDirectory = Path.Combine(REPOSITORY_FOLDER, tmAssignment.tmName);
                        //var dir = FSDirectory.Open(new DirectoryInfo(tmDirectory));
                        // var RAMDir = new MMapDirectory(tmDirectory);
                        //var reader = DirectoryReader.Open(dir);
                        var tmWriter = GetTMWriter(tmAssignment.tmPath);
                        var reader = tmWriter.IndexWriter.GetReader(false);

                        var lstTermNums = new List<int>();
                        var lstSpecialities = new List<String>();
                        //init the doc values lists
                        for (int i = 0; i < reader.MaxDoc; i++)
                        {
                            lstTermNums.Add(i);
                            lstSpecialities.Add("");
                        }

                        // the lookup table. It is faster to read all doc values at the beginning.
                        var leafReaderContexts = reader.Leaves;
                        // get the term nums for the documents
                        var t1 = CATUtils.CurrentTimeMillis();
                        foreach (var lrx in leafReaderContexts)
                        {
                            var lr = lrx.AtomicReader;
                            var ndv = lr.GetNumericDocValues("TermsNum");
                            var bdv = lr.GetBinaryDocValues("Specialities");
                            for (int i = 0; i < lr.MaxDoc; i++)
                            {
                                var docId = lrx.DocBase + i;
                                if (ndv != null)
                                {
                                    int termNum = (int)ndv.Get(i);
                                    lstTermNums[docId] = termNum;
                                }
                                if (bdv != null)
                                {
                                    BytesRef binaryValue = new BytesRef();
                                    bdv.Get(i, binaryValue);
                                    lstSpecialities[docId] = "," + binaryValue.Utf8ToString() + ",";
                                }
                                else
                                    bUseSpeciality = false; //it needs to be backward compatible with the earlier index
                            }
                        }

                        Debug.WriteLine("Init doc values: " + (CATUtils.CurrentTimeMillis() - t1) + "ms.");

                        // term freqs
                        t1 = CATUtils.CurrentTimeMillis();
                        var termFreqs = new Dictionary<String, int>();
                        // var sbTermFreqs = new StringBuilder();
                        foreach (var term in uniqueTerms)
                        {
                            int freq = reader.DocFreq(new Term(searchFieldName, term));
                            termFreqs.Add(term, freq);
                            // sbTermFreqs.Append(term + "\t" + freq + "\n");
                        }
                        Debug.WriteLine("Term freqs: " + (CATUtils.CurrentTimeMillis() - t1) + "ms.");

                        t1 = CATUtils.CurrentTimeMillis();
                        var maxDoc = reader.MaxDoc;
                        var emptyArray = new short[maxDoc];
                        var scoredDocs = new short[maxDoc];
                        var repetitions = new Dictionary<String, int>();
                        //the documents that contains the terms
                        var termsDocuments = new Dictionary<String, List<int>>();
                        for (int i = 0; i < transUnits.Count; i++)
                        {
                            var score = scores[i];
                            if (score.isRepetition)
                                continue;
                            var termList = textFragmentsUniqueTermsList[i];
                            var codedText = transUnits[i].source.GetCodedText();

                            // initialize buffers
                            System.Buffer.BlockCopy(emptyArray, 0, scoredDocs, 0, emptyArray.Length * 2); //short array
                            var docPointers = new List<int>();// new FixedBitSet(maxDoc);
                                                              // order the unique terms by term freq
                                                              // termList.sort((term1, term2) -> termFreqs.get(term1) - termFreqs.get(term2));
                                                              // var orderedTermList = termList.subList(0, (int) (termList.Length *
                                                              // CUT_OFF_RATIO));
                                                              // var roughThresholdFreq = (int) (orderedTermList.Length * ROUGH_CUTOFF);

                            var roughThresholdFreq = (int)(termList.Count * ROUGH_CUTOFF);

                            foreach (var term in termList)
                            {
                                List<int> docIds = null;
                                termsDocuments.TryGetValue(term, out docIds);
                                if (docIds == null)
                                {
                                    docIds = new List<int>();
                                    termsDocuments.Add(term, docIds);
                                    foreach (var lrx in leafReaderContexts)
                                    {
                                        var lr = lrx.AtomicReader;
                                        var docBase = lrx.DocBase;
                                        var docs = lr.GetTermDocsEnum(new Term(searchFieldName, term));
                                        if (docs == null)
                                            continue;
                                        int docId;
                                        while ((docId = docs.NextDoc()) != DocIdSetIterator.NO_MORE_DOCS)
                                        {
                                            docId = docBase + docId;
                                            docIds.Add(docId);
                                            if (docIds.Count > maxDocs)
                                                maxDocs = docIds.Count;
                                            loopCntr1++;
                                            int matches = ++scoredDocs[docId];
                                            if (matches > roughThresholdFreq)
                                                docPointers.Add(docId);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var docId in docIds)
                                    {
                                        loopCntr1++;
                                        int matches = ++scoredDocs[docId];
                                        if (matches > roughThresholdFreq)
                                            docPointers.Add(docId);
                                    }
                                }
                            }

                            float maxScore = 0.0f;
                            if (docPointers.Count > 0)
                            {
                                float quality = 0;
                                int uniqueTermSize = termList.Count;
                                var uniqueDocIds = new HashSet<int>(docPointers);
                                foreach (var docId in uniqueDocIds)
                                {
                                    scoreCntr++;
                                    quality = (float)(2.0f * (float)scoredDocs[docId]
                                            / (float)(lstTermNums[docId] + uniqueTermSize)) * 100.0f;
                                    if (quality > maxScore)
                                    {
                                        if (bUseSpeciality)
                                        {
                                            //check if the speciality exists in the specilities list
                                            if (lstSpecialities[docId].Contains(speciality))
                                                maxScore = quality;
                                            else if (tmAssignment.penaltyForOtherSpecialities > 0)
                                                maxScore = quality - tmAssignment.penaltyForOtherSpecialities;
                                        }
                                        else
                                        {
                                            maxScore = quality;
                                        }
                                    }
                                }
                            }

                            //update the score
                            var hasMatch = maxScore > 0.0f;

                            if (score.score < maxScore)
                                score.score = maxScore;

                            if (tmAssignment.penalty > 0 && hasMatch)
                                score.score -= tmAssignment.penalty;
                        }

                        //prepare the context check
                        if (contextCheckThread != null)
                            contextCheckThread.Join();
                        if (dsContexts != null)
                        {
                            var contextRows = dsContexts.Tables[0].Rows;
                            foreach (DataRow contextRow in contextRows)
                            {
                                var idx = (int)contextRow[0];
                                if (scores[idx].score == 100 || scores[idx].isRepetition)
                                    scores[idx].score = 101;
                            }
                        }

                        System.Diagnostics.Debug.WriteLine("lookup: " + (CATUtils.CurrentTimeMillis() - t1) + "ms.");
                        //System.Diagnostics.Debug.WriteLine("loopCntr1: " + loopCntr1 + " maxDocs: " + maxDocs + " scoreCntr: " + scoreCntr);
                        //logger.Log("Penalties.log", tmAssignment.tmPath + " -> " + tmAssignment.penalty);
                    }
                }

                for (int i = 0; i < scores.Count; i++)
                {
                    var score = scores[i];
                    if (score.isRepetition && score.score < 100)
                        stats.repetitions += score.wordcount;
                    else if (score.score > 100)
                        stats.match_101 += score.wordcount;
                    else if (score.score == 100)
                        stats.match_100 += score.wordcount;
                    else if (score.score >= 95 && score.score < 100)
                        stats.match_95_99 += score.wordcount;
                    else if (score.score >= 85 && score.score < 95)
                        stats.match_85_94 += score.wordcount;
                    else if (score.score >= 75 && score.score < 85)
                        stats.match_75_84 += score.wordcount;
                    else if (score.score >= 50 && score.score < 75)
                        stats.match_50_74 += score.wordcount;
                    else
                        stats.no_match += score.wordcount;
                }

                Debug.WriteLine("Total: " + (CATUtils.CurrentTimeMillis() - lStart) + "ms.");
                return stats;

            }
            catch (Exception ex)
            {
                logger.Log("Statistics.log", "ERROR: " + ex.ToString());
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// CreateXliff
        /// </summary>
        /// <param name="sFileName"></param>
        /// <param name="fileContent"></param>
        /// <param name="sFilterName"></param>
        /// <param name="filterContent"></param>
        /// <param name="sourceLangISO639_1"></param>
        /// <param name="targetLangISO639_1"></param>
        /// <param name="aTMAssignments"></param>
        /// <returns></returns>
        public String CreateXliff(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent,
            String sourceLangISO639_1, String targetLangISO639_1, TMAssignment[] aTMAssignments)
        {
            long lStart = CATUtils.CurrentTimeMillis();
            //pre-process the document
            //fileContent = DocumentProcessor.PreprocessDocument(sFilterName, fileContent);

            var sXliffContent = okapiConnector.CreateXliffFromDocument(sFileName, fileContent, sFilterName,
                filterContent, sourceLangISO639_1, targetLangISO639_1);

            var sPreTranslatedXliff = PreTranslateXliff(sXliffContent, sourceLangISO639_1, targetLangISO639_1, aTMAssignments, 100);
            long lElapsed = CATUtils.CurrentTimeMillis() - lStart;
            //logger.Log("Benchmark.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\tCreateXliff\t" + lElapsed
            //    + "\t" + sFileName + "\t" + sourceLangISO639_1 + "_" + targetLangISO639_1 + String.Join(",", Array.ConvertAll(aTMAssignments, o => o.tmPath)));

            return sPreTranslatedXliff;
        }

        public byte[] CreateDocumentFromXliff(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent,
            String sourceLangISO639_1, String targetLangISO639_1, String sXliffContent)
        {
            long lStart = CATUtils.CurrentTimeMillis();
            byte[] aBytes = okapiConnector.CreateDocumentFromXliff(sFileName, fileContent, sFilterName, filterContent,
                sourceLangISO639_1, targetLangISO639_1, sXliffContent);
            var sExt = Path.GetExtension(sFileName).ToLower();
            if (sExt == ".mqxliff")
            {
                //set the status for memoQ xliff files
                var xliffContent = Encoding.UTF8.GetString(aBytes);
                xliffContent = xliffContent.Replace("mq:status=\"NotStarted\"", "mq:status=\"ManuallyConfirmed\"");
                aBytes = Encoding.UTF8.GetBytes(xliffContent);
            }

            //long lElapsed = CATUtils.CurrentTimeMillis() - lStart;
            //logger.Log("Benchmark.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\tCreateDocumentFromXliff\t" + lElapsed
            //    + "\t" + sFileName + "\t" + sourceLangISO639_1 + "_" + targetLangISO639_1);
            return aBytes;
        }

        /// <summary>
        /// GetExactMatch
        /// </summary>
        /// <param name="aTmAssignments"></param>
        /// <param name="source"></param>
        /// <param name="prev"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private TMMatch GetExactMatch(TMAssignment[] aTmAssignments, String source, String prev, String next)
        {
            DataTable dtExactMatches = null;
            String sourceCoded = "";
            if (!String.IsNullOrEmpty(source))
                sourceCoded = CATUtils.XliffSegmentToTextFragmentSimple(source.Trim()).GetCodedText();
            String prevCoded = "";
            if (!String.IsNullOrEmpty(prev))
                prevCoded = CATUtils.XliffSegmentToTextFragmentSimple(prev.Trim()).GetCodedText();
            String nextCoded = "";
            if (!String.IsNullOrEmpty(next))
                nextCoded = CATUtils.XliffSegmentToTextFragmentSimple(next.Trim()).GetCodedText();
            var context = CATUtils.djb2hash(prevCoded + nextCoded);
            foreach (var tmAssignment in aTmAssignments)
            {
                var dsExactMatches = dbFactory.GetExactMatchesBySource(tmAssignment.tmPath, sourceCoded);
                if (dsExactMatches == null)
                    continue;
                if (dtExactMatches == null)
                    dtExactMatches = dsExactMatches.Tables[0];
                else
                    dtExactMatches.Merge(dsExactMatches.Tables[0]);
            }
            if (dtExactMatches == null)
                return null;

            //pick the newest or the incontext one
            var translation = "";
            var inContextMatches = dtExactMatches.Select("context='" + context.ToString() + "'").ToArray();
            var score = 0;
            if (inContextMatches.Length > 0)
            {
                translation = inContextMatches[0]["target"].ToString();
                score = 101;
            }
            else if (dtExactMatches.Rows.Count > 0)
            {
                translation = dtExactMatches.Rows[0]["target"].ToString();
                score = 100;
            }

            if (!String.IsNullOrEmpty(translation))
            {
                translation = CATUtils.TextFragmentToTmx(new TextFragment(translation));
                return new TMMatch() { source = source, target = translation, origin = "", id = "", quality = score, metadata = null };
            }

            return null;
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
        public String PreTranslateXliff(String sXliffContent, String langFrom_ISO639_1, String langTo_ISO639_1,
            TMAssignment[] aTMAssignments, int matchThreshold)
        {
            var cntr = 0;
            try
            {
                //check the languages for the TMs
                if (aTMAssignments != null)
                {
                    var lstTmpTMs = new List<TMAssignment>();
                    foreach (var tmAssignment in aTMAssignments)
                    {
                        var tmInfo = GetTMInfo(tmAssignment.tmPath, false);
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
                xmlnsManager.AddNamespace("x", xliff.DocumentElement.NamespaceURI);

                //fix the source language
                var fileNode = xliff.SelectSingleNode("//x:file", xmlnsManager);
                fileNode.Attributes["source-language"].Value = langFrom_ISO639_1;

                XmlNodeList tuNodeList = xliff.GetElementsByTagName("trans-unit");
                String prev = null;
                String next = null;
                for (int i = 0; i < tuNodeList.Count; i++)
                {
                    cntr = i;
                    Stopwatch swInner = new Stopwatch();
                    swInner.Start();
                    XmlNode tuNode = tuNodeList[i];

                    //create the target node if it doesn't exists
                    var targetNode = tuNode["target"];
                    if (targetNode == null)
                    {
                        targetNode = xliff.CreateElement("target", xliff.DocumentElement.NamespaceURI);
                        //the target language
                        var xmlAttribute = xliff.CreateAttribute("xml:lang");
                        xmlAttribute.Value = langTo_ISO639_1;
                        targetNode.Attributes.Append(xmlAttribute);
                        tuNode.AppendChild(targetNode);
                    }

                    XmlNodeList segmentNodes = null;
                    var ssNode = tuNode["seg-source"];
                    if (ssNode == null)
                        segmentNodes = tuNode.SelectNodes("x:source", xmlnsManager);
                    else
                        segmentNodes = ssNode.SelectNodes("x:mrk", xmlnsManager);
                    var targetXml = "";
                    for (int j = 0; j < segmentNodes.Count; j++)
                    {
                        XmlNode segmentNode = segmentNodes[j];
                        var source = segmentNode.InnerXml.Trim();
                        var status = segmentNode.Attributes["status"];
                        if (status == null)
                        {
                            status = xliff.CreateAttribute("status");
                            segmentNode.Attributes.Append(status);
                        }

                        var mid = "-1";
                        if (segmentNode.Name == "mrk")
                            mid = segmentNode.Attributes["mid"].Value;
                        //get the next segment for the context
                        if (j < segmentNodes.Count - 1)
                            next = segmentNodes[j + 1].InnerXml.Trim();
                        else
                        {
                            //pick the first segment from the next tu
                            var nextTu = i < tuNodeList.Count - 1 ? tuNodeList[i + 1] : null;
                            if (segmentNode.Name == "mrk")
                                next = nextTu?["seg-source"]?["mrk"]?.InnerXml.Trim();
                            else
                                next = nextTu?["source"].InnerXml.Trim();
                        }

                        XmlNode targetSegment = null;
                        if (segmentNode.Name == "mrk")
                            targetSegment = targetNode.SelectSingleNode("x:mrk[@mid='" + mid + "']", xmlnsManager);
                        else
                            targetSegment = targetNode;
                        if (targetSegment == null || targetSegment.InnerXml.Length == 0)
                        {
                            String sStartingWhiteSpaces = Regex.Match(segmentNode.InnerXml, @"^\s*").Value;
                            String sEndingWhiteSpaces = Regex.Match(segmentNode.InnerXml, @"\s*$").Value;
                            //the translation
                            var tmMatch = GetExactMatch(aTMAssignments, source, prev, next);
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

                String sRet = xliff.OuterXml;

                //LOG("PreTranslate.log", String.Format("End ... min: {0}ms, max: {1}ms, avg: {2}ms total: {3}ms", min, max, total / tuNodeList.Count, total));

                return sRet;
            }
            catch (Exception ex)
            {
                logger.Log("PreTranslate.log", "ERROR: " + ex.ToString());
                throw new Exception(ex.Message);
            }
            finally
            {
            }
        }

        /// <summary>
        /// GetTMInfo
        /// </summary>
        /// <param name="sTMName"></param>
        /// <param name="fullInfo"></param>
        /// <returns></returns>
        public TMInfo GetTMInfo(String tmPath, bool fullInfo)
        {
            try
            {
                var tmName = GetTMName(tmPath);
                var tmInfo = new TMInfo();
                //get the languages from the TM name
                Match m = null;
                if (tmName.StartsWith("$__")) //global
                {
                    m = Regex.Match(tmName, "_(.+)_(.+)");
                    tmInfo.tmType = TMType.global;
                }
                else if (tmName.Contains("_sec_")) //secondary
                {
                    m = Regex.Match(tmName, "_sec_(.+)_(.+)_(.+)");
                    if (tmName.StartsWith("__"))
                        tmInfo.tmType = TMType.profileSecondary;
                    else
                        tmInfo.tmType = TMType.groupSecondary;
                }
                else //primary
                {
                    m = Regex.Match(tmName, "_(.+)_(.+)_(.+)");
                    if (tmName.StartsWith("__"))
                        tmInfo.tmType = TMType.profilePrimary;
                    else
                        tmInfo.tmType = TMType.groupPrimary;
                }
                var langFrom_ISO639_1 = m.Groups[2].Value;
                var lagngTo_ISO639_1 = m.Groups[3].Value;

                tmInfo.tmPath = tmPath;
                tmInfo.langFrom = langFrom_ISO639_1;
                tmInfo.langTo = lagngTo_ISO639_1;

                //additional info
                if (fullInfo)
                {
                    //last access
                    DateTime lastAccess = DateTime.MinValue;
                    var aFilePaths = System.IO.Directory.GetFiles(GetSourceIndexDirectory(tmPath));
                    foreach (String sFilePath in aFilePaths)
                    {
                        var tmpLastAccess = File.GetLastWriteTime(sFilePath);
                        if (tmpLastAccess > lastAccess)
                            lastAccess = tmpLastAccess;
                    }

                    tmInfo.lastAccess = lastAccess;

                    //the entry numbers
                    tmInfo.entryNumber = dbFactory.GetTMEntriesNumber(tmPath);
                }

                return tmInfo;
            }
            catch (Exception ex)
            {
                logger.Log("TMEntries.log", "ERROR: " + ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        private static int CountWords(TextFragment segment)
        {
            String sText = segment.GetText();
            sText = sText.Replace("-", "");
            sText = Regex.Replace(sText, "\\d+\\.\\d+", "000");
            sText = Regex.Replace(sText, "'", "");
            sText = Regex.Replace(sText, "’", "");
            sText = Regex.Replace(sText, @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)", "URL");

            var matches = Regex.Matches(sText, "\\w+");
            int totalWordCount = matches.Count;

            return totalWordCount;
        }

        /// <summary>
        /// GetTMMatches
        /// </summary>
        /// <param name="aTMAssignments"></param>
        /// <param name="sSourceText"></param>
        /// <param name="sPrevText"></param>
        /// <param name="sNextText"></param>ge
        /// <param name="matchThreshold"></param>
        /// <param name="maxHits"></param>
        /// <returns></returns>
        public TMMatch[] GetTMMatches(TMAssignment[] aTMAssignments, String sSourceText, String sPrevText, String sNextText, byte matchThreshold, int maxHits)
        {
            long lStart = CATUtils.CurrentTimeMillis();
            try
            {
                var loopCntr = 0;
                var scoreCntr = 0;
                var ROUGH_CUTOFF = 0.5;
                var lstTMMatches = new List<TMMatch>();

                //we need the source as TextFragment
                var source = CATUtils.TmxSegmentToTextFragmentSimple(sSourceText);
                //the context
                var prev = CATUtils.TmxSegmentToTextFragmentSimple(sPrevText);
                var next = CATUtils.TmxSegmentToTextFragmentSimple(sNextText);
                int context = CATUtils.djb2hash(prev.GetCodedText() + next.GetCodedText());
                var sourceCoded = source.GetCodedText();
                foreach (var tmAssignment in aTMAssignments)
                {
                    var bUseSpeciality = tmAssignment.speciality >= 0;
                    String speciality = "," + tmAssignment.speciality.ToString() + ","; //not too nice but ok
                    //get the connector
                    var tmWriter = GetTMWriter(tmAssignment.tmPath);
                    var reader = tmWriter.IndexWriter.GetReader(false); // No benefit of OpenIfChanged

                    //the unique terms
                    var terms = CATUtils.GetTermsFromText(source.GetText());
                    var uniqeTerms = new HashSet<String>(terms);

                    // initialize buffers
                    var maxDoc = reader.MaxDoc;
                    var scoredDocs = new short[maxDoc];
                    var termNums = new short[maxDoc];
                    var specialities = new String[maxDoc];
                    var docPointers = new List<int>(); // new FixedBitSet(maxDoc);

                    String searchFieldName = TranslationUnitField.SOURCE.ToString();
                    var leafReaderContexts = reader.Leaves;
                    var roughThresholdFreq = (int)(uniqeTerms.Count * ROUGH_CUTOFF);
                    foreach (var term in uniqeTerms)
                    {
                        var docIds = new List<int>();
                        foreach (var lrx in leafReaderContexts)
                        {
                            var lr = lrx.AtomicReader;
                            var docBase = lrx.DocBase;
                            var docs = lr.GetTermDocsEnum(new Term(searchFieldName, term));
                            if (docs == null)
                                continue;
                            var ndv = lr.GetNumericDocValues("TermsNum");
                            var bdv = lr.GetBinaryDocValues("Specialities");
                            if (bdv == null)
                                bUseSpeciality = false; //backward compatibility
                            int docId;
                            while ((docId = docs.NextDoc()) != DocIdSetIterator.NO_MORE_DOCS)
                            {
                                if (bUseSpeciality)
                                {
                                    BytesRef binaryValue = new BytesRef();
                                    bdv.Get(docId, binaryValue);
                                    var storedSpeciality = "," + binaryValue.Utf8ToString() + ",";
                                    specialities[docBase + docId] = storedSpeciality;
                                }
                                docId = docBase + docId;
                                docIds.Add(docId);
                                termNums[docId] = (short)ndv.Get(docId - docBase);

                                loopCntr++;
                                int matches = ++scoredDocs[docId];
                                if (matches >= roughThresholdFreq)
                                    docPointers.Add(docId);
                            }
                        }
                    }

                    var tmHits = new Dictionary<int, float>();
                    if (docPointers.Count > 0)
                    {
                        float quality = 0;
                        int uniqueTermSize = uniqeTerms.Count;
                        //foreach (var docId in docPointers)
                        var uniqueDocIds = new HashSet<int>(docPointers); //there must be a faster way for integers
                        foreach (var docId in uniqueDocIds)
                        {
                            scoreCntr++;
                            quality = (float)(2.0f * (float)scoredDocs[docId]
                                    / (float)(termNums[docId] + uniqueTermSize)) * 100.0f;

                            if (bUseSpeciality)
                            {
                                if (tmAssignment.penaltyForOtherSpecialities > 0)
                                {
                                    //check if the speciality exists in the specilities list
                                    if (!specialities[docId].Contains(speciality))
                                        quality -= tmAssignment.penaltyForOtherSpecialities;
                                }
                                else if (!specialities[docId].Contains(speciality))
                                    continue;
                            }

                            if (quality > matchThreshold)
                            {
                                tmHits.Add(docId, quality);
                            }
                        }
                    }

                    //sort the tm hits
                    var sortedTmHits = from entry in tmHits orderby entry.Value descending select entry;
                    var topHits = sortedTmHits.Take(maxHits).ToList();

                    foreach (var tmHit in topHits)
                    {
                        var doc = reader.Document(tmHit.Key);
                        int idSource = int.Parse(doc.GetField("ID").GetStringValue());
                        var dsTMEntries = dbFactory.GetTMEntriesBySourceIds(tmAssignment.tmPath, new int[] { idSource }); //we could do it faster
                        var aTMEntries = dsTMEntries.Tables[0].Rows;
                        foreach (DataRow tmEntry in aTMEntries)
                        {
                            var tmMatch = new TMMatch();
                            var matchSourceCoded = (String)tmEntry["source"];
                            tmMatch.id = tmEntry["id"].ToString();
                            tmMatch.source = CATUtils.TextFragmentToTmx(new TextFragment(matchSourceCoded));
                            tmMatch.target = CATUtils.TextFragmentToTmx(new TextFragment((String)tmEntry["target"]));
                            tmMatch.quality = (int)tmHit.Value;
                            tmMatch.origin = tmAssignment.tmPath;
                            tmMatch.metadata = GetMetaData(tmEntry, tmAssignment.tmPath);
                            if (tmMatch.quality == 100)
                            {
                                if (context.ToString() == (String)tmEntry["context"])
                                    tmMatch.quality = 101;

                                if (matchSourceCoded != sourceCoded)
                                    tmMatch.quality -= 2; //2% penalty
                            }
                            tmMatch.quality -= tmAssignment.penalty;
                            lstTMMatches.Add(tmMatch);
                        }
                    }
                }

                //sort the result
                lstTMMatches.Sort((x, y) => y.quality.CompareTo(x.quality));
                //remove the duplicates
                //lstTMMatches = from tmMatch in lstTMMatches group tmMatch by tmMatch.target into g select   
                lstTMMatches = lstTMMatches.GroupBy(tmMatch => tmMatch.target).Select(g => g.OrderByDescending(tmMatch => tmMatch.quality).First()).ToList();
                lstTMMatches.Take(maxHits);

                //System.Diagnostics.Debug.WriteLine("Loops:" + loopCntr + " scores: " + scoreCntr);

                long lElapsed = CATUtils.CurrentTimeMillis() - lStart;
                //logger.Log("Benchmark.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\tGetTMEntries\t" + lElapsed);

                return lstTMMatches.ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aTMPaths"></param>
        /// <param name="sSourceText"></param>
        /// <param name="sTargetText"></param>
        /// <param name="bCaseSensitive"></param>
        /// <param name="bNumericEquivalenve"></param>
        /// <param name="nLimit"></param>
        /// <returns></returns>
        public TMEntry[] Concordance(String[] aTMPaths, String sSourceText, String sTargetText, bool bCaseSensitive, int maxHits)
        {
            try
            {
                var lstTMMatches = new List<TMEntry>();

                //search in target
                if (sTargetText != null && sTargetText.Length > 0)
                {
                    foreach (var tmPath in aTMPaths)
                    {
                        var dsTMEntries = dbFactory.GetTMEntriesByTargetText(tmPath, sTargetText);
                        var aTMEntries = dsTMEntries.Tables[0].Rows;
                        foreach (DataRow tmEntry in aTMEntries)
                        {
                            var tmpTmEntry = new TMEntry();
                            var matchSourceCoded = (String)tmEntry["source"];
                            tmpTmEntry.id = (int)tmEntry["id"];
                            tmpTmEntry.source = CATUtils.TextFragmentToTmx(new TextFragment(matchSourceCoded));
                            tmpTmEntry.target = CATUtils.TextFragmentToTmx(new TextFragment((String)tmEntry["target"]));
                            tmpTmEntry.metadata = GetMetaData(tmEntry, tmPath);
                            lstTMMatches.Add(tmpTmEntry);
                        }
                    }

                    lstTMMatches.Take(maxHits);
                    return lstTMMatches.ToArray();
                }

                if (sSourceText.Length < N_GRAM_LEN)
                {
                    sSourceText = sSourceText.PadLeft(sSourceText.Length + 1);
                    if (sSourceText.Length < N_GRAM_LEN)
                        sSourceText = sSourceText.PadRight(sSourceText.Length + 1);
                    if (sSourceText.Length < N_GRAM_LEN)
                        sSourceText = sSourceText.PadLeft(N_GRAM_LEN);
                }

                //search in source
                var loopCntr = 0;
                var scoreCntr = 0;
                var ROUGH_CUTOFF = 0.5;
                //we need the source as TextFragment
                foreach (var tmPath in aTMPaths)
                {
                    //get the connector
                    var tmWriter = GetTMWriter(tmPath);
                    var reader = tmWriter.IndexWriter.GetReader(false); // No benefit of OpenIfChanged

                    //the unique terms
                    var terms = CATUtils.GetTermsFromText(sSourceText);
                    var uniqeTerms = new HashSet<String>(terms);

                    // initialize buffers
                    var maxDoc = reader.MaxDoc;
                    var scoredDocs = new short[maxDoc];
                    var docValues = new short[maxDoc];
                    var docPointers = new List<int>(); // new FixedBitSet(maxDoc);

                    String searchFieldName = TranslationUnitField.SOURCE.ToString();
                    var leafReaderContexts = reader.Leaves;
                    var roughThresholdFreq = (int)(uniqeTerms.Count * ROUGH_CUTOFF);
                    foreach (var term in uniqeTerms)
                    {
                        var docIds = new List<int>();
                        foreach (var lrx in leafReaderContexts)
                        {
                            var lr = lrx.AtomicReader;
                            var docBase = lrx.DocBase;
                            var docs = lr.GetTermDocsEnum(new Term(searchFieldName, term));
                            if (docs == null)
                                continue;
                            var ndv = lr.GetNumericDocValues("TermsNum");
                            int docId;
                            while ((docId = docs.NextDoc()) != DocIdSetIterator.NO_MORE_DOCS)
                            {
                                docId = docBase + docId;
                                docIds.Add(docId);
                                docValues[docId] = (short)ndv.Get(docId - docBase);
                                loopCntr++;
                                int matches = ++scoredDocs[docId];
                                if (matches > roughThresholdFreq)
                                    docPointers.Add(docId);
                            }
                        }
                    }

                    var tmHits = new Dictionary<int, float>();
                    if (docPointers.Count > 0)
                    {
                        float quality = 0;
                        int uniqueTermSize = uniqeTerms.Count;
                        //foreach (var docId in docPointers)
                        var uniqueDocIds = new HashSet<int>(docPointers); //there must be a faster way for integers
                        foreach (var docId in uniqueDocIds)
                        {
                            scoreCntr++;
                            quality = (float)(2.0f * (float)scoredDocs[docId]
                                    / (float)(docValues[docId] + uniqueTermSize)) * 100.0f;
                            tmHits.Add(docId, quality);
                        }
                    }

                    //sort the tm hits
                    var sortedTmHits = from entry in tmHits orderby entry.Value descending select entry;
                    var topHits = sortedTmHits.Take(maxHits).ToList();

                    var lstSourceIds = new List<int>();
                    foreach (var tmHit in topHits)
                    {
                        var doc = reader.Document(tmHit.Key);
                        int idSource = int.Parse(doc.GetField("ID").GetStringValue());
                        lstSourceIds.Add(idSource);
                    }

                    if (lstSourceIds.Count > 0)
                    {
                        var dsTMEntries = dbFactory.GetTMEntriesBySourceIds(tmPath, lstSourceIds.ToArray());
                        var aTMEntries = dsTMEntries.Tables[0].Rows;
                        foreach (DataRow tmEntry in aTMEntries)
                        {
                            var tmpTmEntry = new TMEntry();
                            var matchSourceCoded = (String)tmEntry["source"];
                            tmpTmEntry.id = (int)tmEntry["id"];
                            tmpTmEntry.source = CATUtils.TextFragmentToTmx(new TextFragment(matchSourceCoded));
                            tmpTmEntry.target = CATUtils.TextFragmentToTmx(new TextFragment((String)tmEntry["target"]));
                            tmpTmEntry.metadata = GetMetaData(tmEntry, tmPath);
                            lstTMMatches.Add(tmpTmEntry);
                        }
                    }
                }

                lstTMMatches.Take(maxHits);

                System.Diagnostics.Debug.WriteLine("Loops:" + loopCntr + " scores: " + scoreCntr);

                return lstTMMatches.ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// CalculateContextHashFromMetadata
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private int CalculateContextHashFromMetadata(Dictionary<String, String> metadata)
        {
            if (metadata.ContainsKey("contextHash"))
            {
                int contextHash = 0;
                int.TryParse(metadata["contextHash"], out contextHash);
                return contextHash;
            }

            TextFragment prev = null;
            TextFragment next = null;
            if (metadata.ContainsKey("prevSegment"))
                prev = CATUtils.TmxSegmentToTextFragmentSimple(metadata["prevSegment"]);
            if (metadata.ContainsKey("nextSegment"))
                next = CATUtils.TmxSegmentToTextFragmentSimple(metadata["nextSegment"]);

            return CalculateContextHash(prev, next);
        }

        /// <summary>
        /// CalculateContextHash
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private int CalculateContextHash(TextFragment prev, TextFragment next)
        {
            String sContext = "";
            if (prev != null)
                sContext += prev.GetCodedText().Trim();

            if (next != null)
                sContext += next.GetCodedText().Trim();

            return CATUtils.djb2hash(sContext);
        }

        /// <summary>
        /// AddTMEntries
        /// </summary>
        /// <param name="sTMName"></param>
        /// <param name="tmEntries"></param>
        /// <returns></returns>
        public int AddTMEntries(String tmPath, TMEntry[] tmEntries)
        {
            try
            {
                int itemsAfter = 0;
                int itemsBefore = 0;
                var start = CATUtils.CurrentTimeMillis();
                //check the TM
                if (!TMExists(tmPath))
                {
                    //create the TM
                    CreateTM(tmPath);
                    //throw new Exception("The TM doesn't exist.");
                }

                var transactionOptions = new TransactionOptions();
                transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                transactionOptions.Timeout = TransactionManager.MaximumTimeout;
                TransactionScope tuTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
                using (tuTransaction)
                {
                    var tmWriter = GetTMWriter(tmPath);
                    // get the number of entries before the import
                    itemsBefore = tmWriter.IndexWriter.NumDocs;

                    //import the entries
                    foreach (var tmEntry in tmEntries)
                    {
                        //convert the texts to text fragments
                        var source = CATUtils.TmxSegmentToTextFragmentSimple(tmEntry.source.Trim());
                        var target = CATUtils.TmxSegmentToTextFragmentSimple(tmEntry.target.Trim());
                        //the context
                        var metadata = JsonConvert.DeserializeObject<Dictionary<String, String>>(tmEntry.metadata);
                        var context = CalculateContextHashFromMetadata(metadata);

                        String idUser = "";
                        metadata.TryGetValue("user", out idUser);
                        int speciality = 0;
                        if (metadata.ContainsKey("speciality"))
                            int.TryParse(metadata["speciality"], out speciality);
                        int idTranslation = -1;
                        if (metadata.ContainsKey("idTranslation"))
                            int.TryParse(metadata["idTranslation"], out idTranslation);
                        //insert into SQL server
                        var dsResult = dbFactory.InsertTMEntry(tmPath, source, target, context.ToString(), idUser, speciality, idTranslation,
                            DateTime.Now, DateTime.Now, (metadata.ContainsKey("metadata")? metadata["metadata"] : ""));
                        //var dsResult = dbFactory.InsertTMEntry(tmPath, source, target, context.ToString(), idUser, speciality, idTranslation,
                        //    DateTime.Now, DateTime.Now, ""); //[AM:29/09/2023] hotfix
                        var rowResult = dsResult.Tables[0].Rows[0];
                        var id = (int)rowResult["idSource"];
                        if ((bool)rowResult["isNew"])
                        {
                            //index the source text
                            tmWriter.IndexSource(id, source, speciality.ToString());
                        }
                        else if ((String)rowResult["oldSpecialities"] != (String)rowResult["newSpecialities"])
                        {
                            var oldSpecialities = String.Join(",", ((String)rowResult["oldSpecialities"]).Split(',').Distinct().OrderBy(element => element).ToArray());
                            var newSpecialities = String.Join(",", ((String)rowResult["newSpecialities"]).Split(',').Distinct().OrderBy(element => element).ToArray());
                            if (oldSpecialities != newSpecialities)
                            {
                                tmWriter.Delete(id);
                                tmWriter.IndexSource(id, source, newSpecialities);
                            }
                        }
                    }
                    tuTransaction.Complete();
                    //the items after the import
                    itemsAfter = tmWriter.IndexWriter.NumDocs;
                    tmWriter.Commit();
                }

                long lElapsed = CATUtils.CurrentTimeMillis() - start;
                System.Diagnostics.Debug.WriteLine("AddTMEntries: " + (CATUtils.CurrentTimeMillis() - start) + ".ms");
                /*if (tmEntries.Length == 1)
                    logger.Log("Benchmark.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\tAddTMEntry\t" + lElapsed);
                else
                    logger.Log("Benchmark.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\tAddTMEntries\t" + lElapsed + "\t" + tmEntries.Length);
                */

                //backup
                backupClient.AddTMEntries(tmPath, tmEntries);

                return itemsAfter - itemsBefore;
            }
            catch (Exception ex)
            {
                logger.Log("TMEntries.log", "ERROR: AddTMEntries TM name -> " + tmPath + "\n\n" + ex.ToString());
                throw ex;
            }
            finally
            {
            }
        }

        /// <summary>
        /// DeleteTMEntry
        /// </summary>
        /// <param name="sTMPath"></param>
        /// <param name="idEntry"></param>
        public void DeleteTMEntry(String tmPath, int idEntry)
        {
            try
            {
                var start = CATUtils.CurrentTimeMillis();
                //check the TM
                if (!TMExists(tmPath))
                {
                    //create the TM
                    CreateTM(tmPath);
                    //throw new Exception("The TM doesn't exist.");
                }

                var transactionOptions = new TransactionOptions();
                transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                transactionOptions.Timeout = TransactionManager.MaximumTimeout;
                TransactionScope tuTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
                using (tuTransaction)
                {
                    //delete the entry from the SQL database
                    var luceneId = dbFactory.DeleteTMEntry(tmPath, idEntry);
                    if (luceneId >= 0)
                    {
                        var tmWriter = GetTMWriter(tmPath);
                        //delete the entry from the Lucene index
                        tmWriter.Delete(luceneId);
                        tmWriter.Commit();
                    }
                    tuTransaction.Complete();
                }

                //backup
                //backupClient.DeleteTMEntry(tmPath, idEntry);
            }
            catch (Exception ex)
            {
                logger.Log("TMEntries.log", "ERROR: DeleteTMEntry TM name -> " + tmPath + "\n\n" + ex.ToString());
                throw ex;
            }
            finally
            {
            }
        }

        /// <summary>
        /// UpdateTMEntry
        /// </summary>
        /// <param name="sTMPath"></param>
        /// <param name="idEntry"></param>
        /// <param name="fieldsToUpdate"></param>
        public void UpdateTMEntry(String tmPath, int idEntry, Dictionary<String, String> fieldsToUpdate)
        {
            try
            {
                var start = CATUtils.CurrentTimeMillis();
                //check the TM
                if (!TMExists(tmPath))
                    throw new Exception("The TM doesn't exist.");

                dbFactory.UpdateTMEntry(tmPath, idEntry, fieldsToUpdate);
                System.Diagnostics.Debug.WriteLine("UpdateTMEntry: " + (CATUtils.CurrentTimeMillis() - start) + ".ms");
                //backup
                backupClient.UpdateTMEntry(tmPath, idEntry, fieldsToUpdate);
            }
            catch (Exception ex)
            {
                logger.Log("TMEntries.log", "ERROR: UpdateTMEntry, TM name -> " + tmPath + "\n\n" + ex.ToString());
                throw ex;
            }
            finally
            {
            }
        }

        private String GetTwoLetterLangCode(String langCode)
        {
            //return new CultureInfo(sSourceLangIso639_1).TwoLetterISOLanguageName;
            return langCode.Substring(0, 2).ToLower();
        }
        /// <summary>
        /// ImportTmx
        /// </summary>
        /// <param name="sTMPath"></param>
        /// <param name="sSourceLangIso639_1"></param>
        /// <param name="sTargetLangIso639_1"></param>
        /// <param name="sTMXContent"></param>
        /// <param name="sUser"></param>
        /// <param name="speciality"></param>
        /// <returns></returns>
        public TMImportResult ImportTmx(String tmPath, String sSourceLangIso639_1, String sTargetLangIso639_1, String sTMXContent, 
            String sUser, int speciality)
        {
            int cntr = 0;
            try
            {
                //check the TM
                if (!TMExists(tmPath))
                    CreateTM(tmPath);

                int itemsAfter = 0;
                int itemsBefore = 0;
                // read the entries from the tmx
                var tmx = new XmlDocument();
                tmx.LoadXml(sTMXContent);
                var tus = tmx.GetElementsByTagName("tu");

                var transactionOptions = new TransactionOptions();
                transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                transactionOptions.Timeout = TransactionManager.MaximumTimeout;

                var tmWriter = GetTMWriter(tmPath);
                itemsBefore = itemsAfter = dbFactory.GetTMEntriesNumber(tmPath);

                //the TransactionManager.MaximumTimeout is 10 minutes. We need to use transaction blocks.
                var swTimeout = new Stopwatch();
                var from = 0;
                var srcTwoLetterISO = GetTwoLetterLangCode(sSourceLangIso639_1);
                var targetTwoLetterISO = GetTwoLetterLangCode(sTargetLangIso639_1);
                string formatStringISO8601 = "yyyyMMdd'T'HHmmss'Z'";
                while (from < tus.Count)
                {
                    var bCommitted = false;
                    swTimeout.Start();
                    //the transaction scope
                    TransactionScope tuTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
                    using (tuTransaction)
                    {
                        // get the number of entries before the import
                        for (int i = from; i < tus.Count; i++)
                        {
                            var tu = (XmlElement)tus[i];
                            var tuProps = tu.GetElementsByTagName("prop");
                            //the extension metadata
                            var metadataExtension = new Dictionary<String, String>();
                            //the dates
                            var dateCreated = DateTime.Now;
                            var dateModified = DateTime.Now;
                            if (tu.Attributes["creationdate"] != null)
                                dateCreated = DateTime.ParseExact(tu.Attributes["creationdate"].Value, formatStringISO8601, CultureInfo.InvariantCulture);
                            if (tu.Attributes["changedate"] != null)
                                dateModified = DateTime.ParseExact(tu.Attributes["changedate"].Value, formatStringISO8601, CultureInfo.InvariantCulture);
                            int idTranslation = -1;
                            var context = "0"; //default context
                            var tuSpeciality = speciality;
                            var contextData = new Dictionary<String, String>();
                            foreach (XmlNode tuProp in tuProps)
                            {
                                //context sometimes it is stored in the tuv node
                                if (tuProp.Attributes["type"].Value.ToLower() == "x-context-pre")
                                {
                                    var type = "prevSegment";
                                    var value = tuProp.InnerText.Replace("<seg>", "").Replace("</seg>", "").Trim();

                                    if (contextData.ContainsKey(type))
                                        contextData[type] = value;
                                    else
                                        contextData.Add(type, value );
                                    continue;
                                }
                                else if (tuProp.Attributes["type"].Value.ToLower() == "x-context-post")
                                {
                                    var type = "nextSegment";
                                    var value = tuProp.InnerText.Replace("<seg>", "").Replace("</seg>", "").Trim();

                                    if (contextData.ContainsKey(type))
                                        contextData[type] = value;
                                    else
                                        contextData.Add(type, value);
                                    continue;

                                }
                                else if (tuProp.Attributes["type"].Value.ToLower() == "x-context")
                                {
                                    var type = "contextHash";
                                    var value = tuProp.InnerText.Replace("<seg>", "").Replace("</seg>", "").Trim();

                                    if (contextData.ContainsKey(type))
                                        contextData[type] = value;
                                    else
                                        contextData.Add(type, value);
                                    continue;
                                }

                                //idTranslation
                                if (tuProp.Attributes["type"].Value.ToLower() == "x-translation")
                                {
                                    if (!int.TryParse(tuProp.InnerText, out idTranslation))
                                        idTranslation = -1;
                                    continue;
                                }

                                //speciality
                                if (tuProp.Attributes["type"].Value.ToLower() == "domain")
                                {
                                    //find by value
                                    int foundSpeciality = specialities.FirstOrDefault(x => x.Value == tuProp.InnerText.ToLower()).Key;

                                    if (foundSpeciality > 0)
                                        tuSpeciality = foundSpeciality;
                                    continue;
                                }
                                if (metadataExtension.ContainsKey(tuProp.Attributes["type"].Value))
                                {
                                    metadataExtension[tuProp.Attributes["type"].Value] = tuProp.InnerText;
                                    logger.Log("TMEntries.log", tuProp.Attributes["type"].Value.ToString() + " " + tuProp.InnerText);
                                }
                                else
                                    metadataExtension.Add(tuProp.Attributes["type"].Value, tuProp.InnerText);
                            }

                            var tuvs = tu.GetElementsByTagName("tuv");
                            // create a new entry
                            var tmEntry = new TMEntry();
                            bool bScrLangFound = false;
                            for (int j = 0; j < tuvs.Count; j++)
                            {
                                var tuv = (XmlElement)tuvs[j];
                                var lang = tuv.Attributes["xml:lang"].Value;
                                var langTwoLetterISO = GetTwoLetterLangCode(lang);
                                if (langTwoLetterISO == srcTwoLetterISO && !bScrLangFound)
                                { // does it need to be culture invariant?
                                    bScrLangFound = true;
                                    var segments = tuv.GetElementsByTagName("seg");
                                    if (segments.Count == 0)
                                        continue;
                                    tmEntry.source = segments[0].InnerXml.Trim();
                                    // set the context
                                    var props = tuv.GetElementsByTagName("prop");
                                    for (int k = 0; k < props.Count; k++)
                                    {
                                        var prop = props[k];
                                        var type = "";
                                        if (prop.Attributes["type"].Value.ToLower() == "x-context-pre")
                                            type = "prevSegment";
                                        else if (prop.Attributes["type"].Value.ToLower() == "x-context-post")
                                            type = "nextSegment";
                                        else if (prop.Attributes["type"].Value.ToLower() == "x-context")
                                            type = "contextHash";

                                        var value = prop.InnerText.Replace("<seg>", "").Replace("</seg>", "").Trim();
                                        if (contextData.ContainsKey(type))
                                            contextData[type] = value;
                                        else
                                            contextData.Add(type, value);
                                    }
                                }
                                else if (langTwoLetterISO == targetTwoLetterISO)
                                {
                                    var segments = tuv.GetElementsByTagName("seg");
                                    if (segments.Count == 0)
                                        continue;
                                    tmEntry.target = segments[0].InnerXml.Trim();
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("Should never happen.");
                                }
                            }

                            // check if the entry is valid
                            if (!String.IsNullOrEmpty(tmEntry.source) && !String.IsNullOrEmpty(tmEntry.target))
                            {
                                //convert the texts to text fragments
                                TextFragment source;
                                TextFragment target;

                                try
                                {
                                    source = CATUtils.XliffSegmentToTextFragmentSimple(tmEntry.source);
                                    target = CATUtils.XliffSegmentToTextFragmentSimple(tmEntry.target);
                                }
                                catch (Exception ex)
                                {
                                    logger.Log("TMEntries.log", "ERROR: TM name -> " + tmPath + "\nsource: " + tmEntry.source + "\ntargt: " +
                                        tmEntry.target + "\n\n" + ex.Message);
                                    continue;
                                }

                                //the context
                                if (contextData.Count > 0)
                                {
                                    context = CalculateContextHashFromMetadata(contextData).ToString();
                                }

                                //insert into SQL server
                                var extensionData = JsonConvert.SerializeObject(metadataExtension);
                                var dsResult = dbFactory.InsertTMEntry(tmPath, source, target, context.ToString(), sUser, tuSpeciality, idTranslation, 
                                    dateCreated, dateModified, extensionData);
                                var rowResult = dsResult.Tables[0].Rows[0];
                                var id = (int)rowResult["idSource"];
                                if ((bool)rowResult["isNew"])
                                {
                                    //index the source text
                                    tmWriter.IndexSource(id, source, tuSpeciality.ToString());
                                }
                                else if ((String)rowResult["oldSpecialities"] != (String)rowResult["newSpecialities"])
                                {
                                    var oldSpecialities = String.Join(",", ((String)rowResult["oldSpecialities"]).Split(',').Distinct().OrderBy(element => element).ToArray());
                                    var newSpecialities = String.Join(",", ((String)rowResult["newSpecialities"]).Split(',').Distinct().OrderBy(element => element).ToArray());
                                    if (oldSpecialities != newSpecialities)
                                    {
                                        tmWriter.Delete(id);
                                        tmWriter.IndexSource(id, source, newSpecialities);
                                    }
                                }
                            }

                            if (swTimeout.ElapsedMilliseconds > 2 * 60 * 1000) //commit in every 2 minutes
                            {
                                cntr++;
                                swTimeout.Reset();
                                tuTransaction.Complete();
                                tmWriter.Commit();
                                tmWriter = GetTMWriter(tmPath); //keep alive
                                from = i + 1;
                                bCommitted = true;
                                break;
                            }
                        }

                        if (!bCommitted)
                        {
                            cntr++;
                            //the final commit
                            tuTransaction.Complete();
                            tmWriter.Commit();
                            bCommitted = true;
                            break;
                        }
                    }
                }

                //shrink the sql database
                ShrinkTM(tmPath);
                //dbFactory.shrinkDatabase(tmPath);
                //tmWriter.IndexWriter.MaybeMerge();

                //the items after the import
                itemsAfter = dbFactory.GetTMEntriesNumber(tmPath);

                var ret = new TMImportResult();
                ret.allItems = itemsAfter;
                ret.importedItems = itemsAfter - itemsBefore;

                //backup
                backupClient.ImportTmx(tmPath, sSourceLangIso639_1, sTargetLangIso639_1, sTMXContent, sUser, speciality);

                return ret;
            }
            catch (Exception ex)
            {
                logger.Log("TMEntries.log", "ERROR: TM name -> " + tmPath + "\n\n" + ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// GetTMList
        /// </summary>
        /// <returns></returns>
        public TMInfo[] GetTMList(bool bFullInfo)
        {
            var lstTMInfo = new List<TMInfo>();
            var aDirs = System.IO.Directory.GetDirectories(REPOSITORY_FOLDER, "*", SearchOption.AllDirectories);
            foreach (String sDir in aDirs)
            {
                var tmDir = Path.GetFileName(sDir);
                if (tmDir.StartsWith("_"))
                {
                    var sCorporateId = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(sDir)));
                    var tmInfo = GetTMInfo(sCorporateId + "/" + tmDir, bFullInfo);
                    lstTMInfo.Add(tmInfo);
                }

            }

            return lstTMInfo.ToArray();
        }

        /// <summary>
        /// GetTMListFromDatabase
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public TMInfo[] GetTMListFromDatabase(String dbName)
        {
            //get the TM tables from the database
            var dsTMNames = dbFactory.GetTMListFromDatabase(dbName);
            var lstTMInfo = new List<TMInfo>();
            foreach (DataRow tmRow in dsTMNames.Tables[0].Rows)
            {
                var tmName = (String)tmRow["TABLE_NAME"];
                var tmInfo = GetTMInfo(dbName + "/" + tmName, false);
                lstTMInfo.Add(tmInfo);
            }

            return lstTMInfo.ToArray();
        }

        public void DeleteDocumentsFromSourceIndex(int[] aDocIds)
        {
            throw new NotImplementedException();
        }

        public String ConnectionPoolInfo()
        {
            var activeWriters = new Dictionary<String, String>();
            //get the TM writers
            foreach (var key in TM_CONNECTION_POOL.Keys)
            {
                String sTmWriter = "Created: " + TM_CONNECTION_POOL[key].created.ToString() +
                    " Last access: " + TM_CONNECTION_POOL[key].lastAccess.ToString();
                activeWriters.Add(key, sTmWriter);
            }

            return JsonConvert.SerializeObject(activeWriters);
        }
        /// <summary>
        /// GetTMWriter
        /// </summary>
        /// <param name="sTMName"></param>
        /// <returns></returns>
        private TMWriter GetTMWriter(String tmPath)
        {
            lock (TM_LOCK)
            {
                try
                {
                    TMWriter tmWriter = null;
                    var sourceIndexDir = GetSourceIndexDirectory(tmPath);
                    // check if there is open connection
                    if (TM_CONNECTION_POOL.ContainsKey(tmPath))
                    {
                        var tmWriterContainer = TM_CONNECTION_POOL[tmPath];
                        tmWriterContainer.lastAccess = DateTime.Now;
                        tmWriter = tmWriterContainer.tmWriter;
                    }
                    else
                    {
                        TM_CONNECTION_POOL = TM_CONNECTION_POOL.OrderBy(x => x.Value.lastAccess).ToDictionary(x => x.Key, x => x.Value);
                        var keysToRemove = new HashSet<String>();
                        //kick out the connectors that older than 20 minutes
                        foreach (var key in TM_CONNECTION_POOL.Keys)
                        {
                            if (TM_CONNECTION_POOL[key].lastAccess < DateTime.Now.AddMinutes(-1 * TM_WRITER_IDLE_TIMEOUT))
                                keysToRemove.Add(TM_CONNECTION_POOL.Keys.First());
                        }
                        //kick out the oldest writer
                        if (TM_CONNECTION_POOL.Count - keysToRemove.Count >= MAX_TM_CONNECTION_POOL_SIZE)
                            keysToRemove.Add(TM_CONNECTION_POOL.Keys.First());

                        foreach (var key in keysToRemove)
                        {
                            try { TM_CONNECTION_POOL[key].tmWriter.Close(); } catch (Exception ex) { }
                            TM_CONNECTION_POOL.Remove(key);
                        }

                        // create new connection
                        tmWriter = TmWriterFactory.CreateFileBasedTmWriter(sourceIndexDir, false);
                        TMConnector tmConnector = new TMConnector();
                        tmConnector.tmPath = tmPath;
                        tmConnector.tmWriter = tmWriter;
                        tmConnector.created = DateTime.Now;
                        tmConnector.lastAccess = DateTime.Now;

                        TM_CONNECTION_POOL.Add(tmPath, tmConnector);

                        //set database last access
                        dbFactory.SetDatabaseLastAccess(GetDatabaseName(tmPath));
                    }

                    return tmWriter;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// ReindexTM
        /// </summary>
        /// <param name="sTMName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int ReindexTM(String tmPath, TMIndex index)
        {
            try
            {
                if (index == TMIndex.Source || index == TMIndex.Both) //the source index
                {
                    //get the data for the source
                    var dsIndexData = dbFactory.GetSourceIndexData(tmPath);
                    var indexData = dsIndexData.Tables[0].Rows;
                    //the TM writer
                    var sourceIndexDir = GetSourceIndexDirectory(tmPath);
                    var tmRoot = GetTMDirectory(tmPath);
                    if (!System.IO.Directory.Exists(tmRoot))
                        System.IO.Directory.CreateDirectory(tmRoot);
                    var indexDir = Path.Combine(tmRoot, "source indexes");
                    if (!System.IO.Directory.Exists(indexDir))
                        System.IO.Directory.CreateDirectory(indexDir);
                    var tmName = GetTMName(tmPath);
                    var tmDir = Path.Combine(indexDir, tmName);
                    if (!System.IO.Directory.Exists(tmDir))
                        System.IO.Directory.CreateDirectory(tmDir);

                    //close the connection
                    if (TM_CONNECTION_POOL.ContainsKey(tmPath))
                    {
                        TM_CONNECTION_POOL[tmPath].tmWriter.Close();
                        TM_CONNECTION_POOL.Remove(tmPath);
                    }

                    var tmWriter = TmWriterFactory.CreateFileBasedTmWriter(sourceIndexDir, true);
                    tmWriter.Commit();

                    var sourceLookup = new Dictionary<int, dynamic>();
                    foreach (DataRow drSource in indexData)
                    {
                        var source = (String)drSource["source"];
                        var id = (int)drSource["idSource"];
                        var speciality = (int)drSource["speciality"];
                        if (sourceLookup.Keys.Contains(id))
                        {
                            //check the speciality
                            var specialities = sourceLookup[id].specialities;
                            if (!specialities.Contains(speciality))
                                specialities.Add(speciality);
                            continue;
                        }
                        sourceLookup.Add(id, new { source, specialities = new List<int>() { speciality } });
                    }

                    //write the Lucene index
                    foreach (var sourceItem in sourceLookup)
                    {
                        var sourceData = sourceItem.Value;
                        sourceData.specialities.Sort();
                        var specialities = String.Join(",", sourceData.specialities.ToArray());
                        tmWriter.IndexSource(sourceItem.Key, new TextFragment(sourceData.source), specialities);
                    }

                    var itemsAfter = tmWriter.IndexWriter.NumDocs;
                    tmWriter.Commit();
                    tmWriter.Close();

                    return itemsAfter;
                }
                else
                    throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                logger.Log("TMEntries.log", "ERROR: ReindexTM TM name -> " + tmPath + "\n\n" + ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// ShrinkTM
        /// </summary>
        /// <param name="sTMName"></param>
        /// <returns></returns>
        public void ShrinkTM(String tmPath)
        {
            try
            {
                //shrink the sql data
                var aPathElements = tmPath.Split('/');
                var dbName = aPathElements[0];
                dbFactory.ShrinkDatabase(dbName);

                //optimize the lucene index
                var tmWriter = GetTMWriter(tmPath);
                tmWriter.IndexWriter.MaybeMerge();

            }
            catch (Exception ex)
            {
                logger.Log("TMEntries.log", "ERROR: shrinkDatabase TM name -> " + tmPath + "\n\n" + ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// ExportTmx
        /// </summary>
        /// <param name="sTMPath"></param>
        /// <returns></returns>
        public String ExportTmx(String tmPath)
        {
            try
            {
                //load the entries
                var dsEntries = dbFactory.GetTranslationMemoryData(tmPath);
                var drEntries = dsEntries.Tables[0].Rows;
                var tmInfo = GetTMInfo(tmPath, false);
                var sbTmx = new StringBuilder();
                sbTmx.Append("<tmx version=\"1.4\">\r\n" + "<header creationtool=\"tm\" segtype=\"sentence\" srclang=\"" + tmInfo.langFrom +
                    "\" datatype=\"unknown\">\r\n"); 
                sbTmx.Append("<prop type=\"name\">" + tmPath + "</prop>\r\n</header>\r\n\t");
                var sbTmxBody = new StringBuilder();
                foreach (DataRow drEntry in drEntries)
                {
                    var tu = "\r\n\t\t<tu tuid=\"" + (int)drEntry["id"] + "\" changedate=\"" + ((DateTime)drEntry["dateModified"]).ToString("yyyyMMdd'T'HHmmss'Z'") + 
                        "\" creationdate=\"" +
                        ((DateTime)drEntry["dateCreated"]).ToString("yyyyMMdd'T'HHmmss'Z'") + "\" creationid=\"" + (String)drEntry["createdBy"] + "\" changeid=\"" + (String)drEntry["modifiedBy"] +
                        "\">\r\n{0}\r\n\t\t</tu>";
                    var sbTuContent = new StringBuilder();
                    //speciality
                    var idSpeciality = drEntry["speciality"] != DBNull.Value ? (int)drEntry["speciality"] : -1;
                    if (specialities.ContainsKey(idSpeciality))
                        sbTuContent.Append("\t\t\t<prop type=\"domain\">" + specialities[idSpeciality] + "</prop>\r\n");

                    int idTranslation = drEntry["idTranslation"] != DBNull.Value ? (int)drEntry["idTranslation"] : -1;
                    if (idTranslation > 0)
                        sbTuContent.Append("\t\t\t<prop type=\"x-translation\">" + idTranslation + "</prop>\r\n");

                    //the extension data
                    var sExtensionData = drEntry["extensionData"] != DBNull.Value ? (String)drEntry["extensionData"] : "";
                    if (sExtensionData != "")
                    {
                        try
                        {
                            var extensionData = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<String, String>>(sExtensionData);
                            foreach (var prop in extensionData.Keys)
                                sbTuContent.Append("\t\t\t<prop type=\"" + prop + "\">" + extensionData[prop] + "</prop>\r\n");
                        }
                        catch (Exception ex)
                        { 
                        }
                    }

                    sbTuContent.Append("\t\t\t<tuv xml:lang=\"" + tmInfo.langFrom + "\">\r\n\t\t\t\t<prop type=\"x-context\">" + (String)drEntry["context"] +
                        "</prop>\r\n\t\t\t\t<seg>" + CATUtils.TextFragmentToTmx(new TextFragment((String)drEntry["source"])) + "</seg>\r\n\t\t\t</tuv>\r\n");
                    sbTuContent.Append("\t\t\t<tuv xml:lang=\"" + tmInfo.langTo + "\">\r\n\t\t\t\t<seg>" + CATUtils.TextFragmentToTmx(new TextFragment((String)drEntry["target"])) +
                        "</seg>\r\n\t\t\t</tuv>");
                    tu = String.Format(tu, sbTuContent.ToString());
                    sbTmxBody.Append(tu);
                }
                sbTmx.Append("<body>" + sbTmxBody + "\r\n\t</body>\r\n</tmx>");

                return sbTmx.ToString();
            }
            catch (Exception ex)
            {
                logger.Log("TMEntries.log", "ERROR: shrinkDatabase TM name -> " + tmPath + "\n\n" + ex.ToString());
                throw ex;
            }
        }

        #region Misc.
        private String GetMetaData(DataRow tmEntry, String tmPath)
        {
            try
            {
                var metadata = new Dictionary<String, String>();
                metadata.Add("origin", tmPath);
                metadata.Add("dateCreated", tmEntry["dateCreated"] != DBNull.Value ? ((DateTime)tmEntry["dateCreated"]).ToString("M/d/yyyy h:mm:ss tt") : "N/A");
                metadata.Add("createdBy", tmEntry["createdBy"] != DBNull.Value ? (String)tmEntry["createdBy"] : "N/A");
                metadata.Add("dateModified", tmEntry["dateModified"] != DBNull.Value ? ((DateTime)tmEntry["dateModified"]).ToString("M/d/yyyy h:mm:ss tt") : "N/A");
                metadata.Add("modifiedBy", tmEntry["modifiedBy"] != DBNull.Value ? (String)tmEntry["modifiedBy"] : "N/A");
                metadata.Add("speciality", tmEntry["speciality"] != DBNull.Value ? ((int)tmEntry["speciality"]).ToString() : "N/A");
                metadata.Add("idTranslation", tmEntry["idTranslation"] != DBNull.Value ? ((int)tmEntry["idTranslation"]).ToString() : "N/A");

                //the extension data
                try
                {
                    metadata.Add("extensionData", tmEntry["extensionData"] != DBNull.Value ? (String)tmEntry["extensionData"] : "");
                }
                catch (Exception ex)
                {
                    metadata.Add("extensionData", "");
                }

                var jsonMetadata = JsonConvert.SerializeObject(metadata);

                return jsonMetadata;
            }
            catch (Exception ex)
            {
                //logger.Log("Metadata.log", "Erros: " + ex.ToString());
            }

            return "";
        }
        #endregion
    }
}
