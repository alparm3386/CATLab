using CAT.BusinessServices;
using CAT.Enums;
using CAT.Models;
using CAT.Utils;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Newtonsoft.Json;
using CAT.Okapi.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Transactions;
using System.Xml;
using System.Text.RegularExpressions;

namespace CAT.TM
{
    public partial class TMService : ITMService
    {
        private readonly object TMLock = new();
        private readonly int TMWriterIdleTimeout = 20; //minutes
        private readonly int MaxTmConnectionPoolSize = 40;
        private Dictionary<string, TMConnector> TMConnectionPool;
        private readonly string RepositoryFolder;
        private readonly ILogger _logger;
        private readonly IDataStorage _dataStorage;
        private readonly int NGramLength = 4;
        private readonly Dictionary<int, string> _specialities;

        public TMService(IDataStorage dataStorage, IConfiguration configuration, ILogger<TMService> logger)
        {
            _dataStorage = dataStorage;
            _logger = logger;

            //initialization
            TMConnectionPool = new Dictionary<string, TMConnector>();
            _specialities = new Dictionary<int, string>(); //read from the database

            RepositoryFolder = configuration["TMPath"]!;
        }

        private string GetSourceIndexDirectory(string tmId)
        {
            var tmName = tmId.Split('/')[1];
            var tmDir = GetTMDirectory(tmId);
            var sourceIndexDir = Path.Combine(tmDir, "source indexes/" + tmName);

            return sourceIndexDir;
        }

        private string GetTMName(string tmId)
        {
            var tmName = tmId.Split('/')[1];
            return tmName;
        }

        private string GetTMDirectory(string tmId)
        {
            var parent = tmId.Split('/')[0];
            var tmDir = Path.Combine(RepositoryFolder, parent);

            return tmDir;
        }

        private static string GetDatabaseName(string tmId)
        {
            return tmId.Split('/')[0];
        }


        /// <summary>
        /// GetStatisticsForTranslationUnits
        /// </summary>
        /// <param name="textFragments"></param>
        /// <param name="aTMAssignments"></param>
        /// <param name="bWithHomogeneity"></param>
        /// <returns></returns>
        public Statistics GetStatisticsForTranslationUnits(List<TranslationUnit> transUnits, string sourceLangISO639_1,
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
                var ROUGH_CUTOFF = 0.5;
                string searchFieldName = "SOURCE";
                // inverted index on source
                var textFragmentsUniqueTermsList = new List<List<string>>();
                var scores = new List<Score>();
                var homogeneityIndex = new InvertedIndex();
                var repetitionsTable = new Dictionary<string, int>();
                //data table for the incontext matches
                var dtQuery = new DataTable();
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
                    repetitionsTable.TryGetValue(codedText, out int idx);
                    //the incontext query
                    dtQuery.Rows.Add(i, codedText, CATUtils.djb2hash(codedText), tu.context);
                    if (idx > 0)
                    {
                        scores.Add(scores[idx - 1].CloneAsRepetition());
                        continue;
                    }
                    else
                        repetitionsTable.Add(codedText, i + 1); //we cannot use zero based index

                    // homogeneity check
                    float highestScore = 0;
                    if (bWithHomogeneity)
                        highestScore = homogeneityIndex.GetHighestScore(text, 50);
                    // the word count
                    var words = wordCounter.CountWords(text);
                    var score = new Score(i, words, highestScore, false);
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
                        string speciality = "," + tmAssignment.speciality.ToString() + ","; //not too nice but ok
                        //get the incontext matches in a background thread
                        Thread contextCheckThread = default!;
                        DataSet dsContexts = default!;
                        if (tmAssignment.penalty <= 2)
                        {
                            contextCheckThread = new Thread(() =>
                            {
                                try
                                {
                                    dsContexts = _dataStorage.CheckIncontextMatches(tmAssignment.tmId, dtQuery);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError("Statistics.log", "ERROR: CheckIncontextMatches -> " + ex.ToString());
                                }
                            });
                            contextCheckThread.Start();
                        }

                        var tmWriter = GetTMWriter(tmAssignment.tmId);
                        var reader = tmWriter.IndexWriter.GetReader(false);

                        var lstTermNums = new List<int>();
                        var lstSpecialities = new List<string>();
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
                                    var binaryValue = new BytesRef();
                                    bdv.Get(i, binaryValue);
                                    lstSpecialities[docId] = "," + binaryValue.Utf8ToString() + ",";
                                }
                                else
                                    bUseSpeciality = false; //it needs to be backward compatible with the earlier index
                            }
                        }

                        Debug.WriteLine("Init doc values: " + (CATUtils.CurrentTimeMillis() - t1) + "ms.");

                        // term freqs
                        var termFreqs = new Dictionary<string, int>();
                        foreach (var term in uniqueTerms)
                        {
                            int freq = reader.DocFreq(new Term(searchFieldName, term));
                            termFreqs.Add(term, freq);
                        }
                        Debug.WriteLine("Term freqs: " + (CATUtils.CurrentTimeMillis() - t1) + "ms.");

                        var maxDoc = reader.MaxDoc;
                        var emptyArray = new short[maxDoc];
                        var scoredDocs = new short[maxDoc];
                        //the documents that contains the terms
                        var termsDocuments = new Dictionary<string, List<int>>();
                        for (int i = 0; i < transUnits.Count; i++)
                        {
                            var score = scores[i];
                            if (score.isRepetition)
                                continue;
                            var termList = textFragmentsUniqueTermsList[i];

                            // initialize buffers
                            Buffer.BlockCopy(emptyArray, 0, scoredDocs, 0, emptyArray.Length * 2); //short array
                            var docPointers = new List<int>();// new FixedBitSet(maxDoc)
                                                              // order the unique terms by term freq
                                                              // termList.sort((term1, term2) -> termFreqs.get(term1) - termFreqs.get(term2))
                                                              // var orderedTermList = termList.subList(0, (int) (termList.Length *
                                                              // CUT_OFF_RATIO))
                                                              // var roughThresholdFreq = (int) (orderedTermList.Length * ROUGH_CUTOFF)

                            var roughThresholdFreq = (int)(termList.Count * ROUGH_CUTOFF);

                            foreach (var term in termList)
                            {
                                List<int> docIds;
                                termsDocuments.TryGetValue(term, out docIds!);
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
                                    quality = 2.0f * scoredDocs[docId]
                                            / (lstTermNums[docId] + uniqueTermSize) * 100.0f;
                                    if (quality > maxScore)
                                    {
                                        if (bUseSpeciality)
                                        {
                                            //check if the speciality exists in the specilities list
                                            if (lstSpecialities[docId].Contains(speciality))
                                                maxScore = quality;
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
                        contextCheckThread?.Join();
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

                return stats;

            }
            catch (Exception ex)
            {
                _logger.LogError("Statistics.log", "ERROR: " + ex.ToString());
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// CreateTM
        /// </summary>
        /// <param name="tmId"></param>
        /// <returns></returns>
        public void CreateTM(string tmId)
        {
            try
            {
                //check if the TM Exists
                if (TMExists(tmId))
                    return;

                //check the TM directory
                var tmDir = GetTMDirectory(tmId);
                if (!System.IO.Directory.Exists(tmDir))
                    System.IO.Directory.CreateDirectory(tmDir);

                //create the SQL repository
                _dataStorage.CreateTranslationMemory(tmId);

                //create the lucene repository
                var sourceIndexDirectory = GetSourceIndexDirectory(tmId);
                if (!System.IO.Directory.Exists(sourceIndexDirectory))
                {
                    System.IO.Directory.CreateDirectory(sourceIndexDirectory);
                    var tmWriter = TmWriterFactory.CreateFileBasedTmWriter(sourceIndexDirectory, true);
                    tmWriter.Commit();
                    tmWriter.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR: CreateTM -> TM name: {TmId}\n{Exception}", tmId, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// TMExists
        /// </summary>
        /// <param name="sTMName"></param>
        /// <returns></returns>
        public bool TMExists(string tmId)
        {
            //the TM directory
            var tmDir = GetTMDirectory(tmId);
            if (!System.IO.Directory.Exists(tmDir))
                return false;

            //the TM table
            if (!_dataStorage.TMExists(tmId))
                return false;

            //the source index directory
            var sourceIndexDirectory = GetSourceIndexDirectory(tmId);
            if (!System.IO.Directory.Exists(sourceIndexDirectory))
                return false;

            return true;
        }

        /// <summary>
        /// GetExactMatch
        /// </summary>
        /// <param name="aTmAssignments"></param>
        /// <param name="source"></param>
        /// <param name="prev"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public TMMatch GetExactMatch(TMAssignment[] aTmAssignments, string source, string prev, string next)
        {
            DataTable dtExactMatches = default!;
            string sourceCoded = "";
            if (!string.IsNullOrEmpty(source))
                sourceCoded = CATUtils.XliffSegmentToTextFragmentSimple(source.Trim()).GetCodedText();
            string prevCoded = "";
            if (!string.IsNullOrEmpty(prev))
                prevCoded = CATUtils.XliffSegmentToTextFragmentSimple(prev.Trim()).GetCodedText();
            string nextCoded = "";
            if (!string.IsNullOrEmpty(next))
                nextCoded = CATUtils.XliffSegmentToTextFragmentSimple(next.Trim()).GetCodedText();
            var context = CATUtils.djb2hash(prevCoded + nextCoded);
            foreach (var tmAssignment in aTmAssignments)
            {
                var dsExactMatches = _dataStorage.GetExactMatchesBySource(tmAssignment.tmId, sourceCoded);
                if (dsExactMatches == null)
                    continue;
                if (dtExactMatches == null)
                    dtExactMatches = dsExactMatches.Tables[0];
                else
                    dtExactMatches.Merge(dsExactMatches.Tables[0]);
            }
            if (dtExactMatches == null)
                return null!;

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

            if (!string.IsNullOrEmpty(translation))
            {
                translation = CATUtils.TextFragmentToTmx(new TextFragment(translation));
                return new TMMatch() { source = source, target = translation, origin = "", id = "", quality = score, metadata = default! };
            }

            return null!;
        }

        /// <summary>
        /// GetTMInfo
        /// </summary>
        /// <param name="sTMName"></param>
        /// <param name="fullInfo"></param>
        /// <returns></returns>
        public TMInfo GetTMInfo(string id, bool fullInfo)
        {
            try
            {
                var tmName = GetTMName(id);
                var tmInfo = new TMInfo();
                //get the languages from the TM name
                Match m = default!;
                if (tmName.StartsWith("$__")) //global
                {
                    m = TMAttributesRegex().Match(tmName);
                    tmInfo.tmType = TMType.global;
                }
                else if (tmName.Contains("_sec_")) //secondary
                {
                    m = SecondaryTMAttributesRegex().Match(tmName);
                    if (tmName.StartsWith("__"))
                        tmInfo.tmType = TMType.profileSecondary;
                    else
                        tmInfo.tmType = TMType.groupSecondary;
                }
                else //primary
                {
                    m = PrimaryTMAttributesRegex().Match(tmName);
                    if (tmName.StartsWith("__"))
                        tmInfo.tmType = TMType.profilePrimary;
                    else
                        tmInfo.tmType = TMType.groupPrimary;
                }
                var langFrom_ISO639_1 = m.Groups[2].Value;
                var lagngTo_ISO639_1 = m.Groups[3].Value;

                tmInfo.tmId = id;
                tmInfo.langFrom = langFrom_ISO639_1;
                tmInfo.langTo = lagngTo_ISO639_1;

                //additional info
                if (fullInfo)
                {
                    //last access
                    DateTime lastAccess = DateTime.MinValue;
                    var aFilePaths = System.IO.Directory.GetFiles(GetSourceIndexDirectory(id));
                    foreach (string sFilePath in aFilePaths)
                    {
                        var tmpLastAccess = File.GetLastWriteTime(sFilePath);
                        if (tmpLastAccess > lastAccess)
                            lastAccess = tmpLastAccess;
                    }

                    tmInfo.lastAccess = lastAccess;

                    //the entry numbers
                    tmInfo.entryNumber = _dataStorage.GetTMEntriesNumber(id);
                }

                return tmInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError("TMEntries.log", "ERROR: " + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// GetTMMatches
        /// </summary>
        /// <param name="aTMAssignments"></param>
        /// <param name="sourceText"></param>
        /// <param name="prevText"></param>
        /// <param name="nextText"></param>ge
        /// <param name="matchThreshold"></param>
        /// <param name="maxHits"></param>
        /// <returns></returns>
        public TMMatch[] GetTMMatches(TMAssignment[] aTMAssignments, string sourceText, string prevText, string nextText, byte matchThreshold, int maxHits)
        {
            try
            {
                var loopCntr = 0;
                var scoreCntr = 0;
                const double RoughCutoff = 0.5;
                var lstTMMatches = new List<TMMatch>();

                //we need the source as TextFragment
                var source = CATUtils.TmxSegmentToTextFragmentSimple(sourceText);
                //the context
                var prev = CATUtils.TmxSegmentToTextFragmentSimple(prevText);
                var next = CATUtils.TmxSegmentToTextFragmentSimple(nextText);
                int context = CATUtils.djb2hash(prev.GetCodedText() + next.GetCodedText());
                var sourceCoded = source.GetCodedText();
                foreach (var tmAssignment in aTMAssignments)
                {
                    var bUseSpeciality = tmAssignment.speciality >= 0;
                    string speciality = "," + tmAssignment.speciality.ToString() + ","; //not too nice but ok
                    //get the connector
                    var tmWriter = GetTMWriter(tmAssignment.tmId);
                    var reader = tmWriter.IndexWriter.GetReader(false); // No benefit of OpenIfChanged

                    //the unique terms
                    var terms = CATUtils.GetTermsFromText(source.GetText());
                    var uniqeTerms = new HashSet<string>(terms);

                    // initialize buffers
                    var maxDoc = reader.MaxDoc;
                    var scoredDocs = new short[maxDoc];
                    var termNums = new short[maxDoc];
                    var specialities = new string[maxDoc];
                    var docPointers = new List<int>(); // new FixedBitSet(maxDoc)

                    string searchFieldName = TranslationUnitField.SOURCE.ToString();
                    var leafReaderContexts = reader.Leaves;
                    var roughThresholdFreq = (int)(uniqeTerms.Count * RoughCutoff);
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
                                    var binaryValue = new BytesRef();
                                    bdv!.Get(docId, binaryValue);
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
                            quality = 2.0f * scoredDocs[docId]
                                    / (termNums[docId] + uniqueTermSize) * 100.0f;

                            if (bUseSpeciality && !specialities[docId].Contains(speciality))
                                continue;

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
                        var dsTMEntries = _dataStorage.GetTMEntriesBySourceIds(tmAssignment.tmId, new int[] { idSource }); //we could do it faster
                        var aTMEntries = dsTMEntries.Tables[0].Rows;
                        foreach (DataRow tmEntry in aTMEntries)
                        {
                            var tmMatch = new TMMatch();
                            var matchSourceCoded = (string)tmEntry["source"];
                            tmMatch.id = tmEntry!["id"]!.ToString()!;
                            tmMatch.source = CATUtils.TextFragmentToTmx(new TextFragment(matchSourceCoded));
                            tmMatch.target = CATUtils.TextFragmentToTmx(new TextFragment((string)tmEntry["target"]));
                            tmMatch.quality = (int)tmHit.Value;
                            tmMatch.origin = tmAssignment.tmId;
                            tmMatch.metadata = GetMetaData(tmEntry, tmAssignment.tmId);
                            if (tmMatch.quality == 100)
                            {
                                if (context.ToString() == (string)tmEntry["context"])
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
                lstTMMatches = lstTMMatches.Take(maxHits).ToList();

                return lstTMMatches.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tmIds"></param>
        /// <param name="sourceText"></param>
        /// <param name="targetText"></param>
        /// <param name="caseSensitive"></param>
        /// <param name="bNumericEquivalenve"></param>
        /// <param name="nLimit"></param>
        /// <returns></returns>
        public TMEntry[] Concordance(string[] tmIds, string sourceText, string targetText, bool caseSensitive, int maxHits)
        {
            try
            {
                var lstTMMatches = new List<TMEntry>();

                //search in target
                if (targetText != null && targetText.Length > 0)
                {
                    foreach (var tmId in tmIds)
                    {
                        var dsTMEntries = _dataStorage.GetTMEntriesByTargetText(tmId, targetText);
                        var aTMEntries = dsTMEntries.Tables[0].Rows;
                        foreach (DataRow tmEntry in aTMEntries)
                        {
                            var tmpTmEntry = new TMEntry();
                            var matchSourceCoded = (string)tmEntry["source"];
                            tmpTmEntry.id = (int)tmEntry["id"];
                            tmpTmEntry.source = CATUtils.TextFragmentToTmx(new TextFragment(matchSourceCoded));
                            tmpTmEntry.target = CATUtils.TextFragmentToTmx(new TextFragment((string)tmEntry["target"]));
                            tmpTmEntry.metadata = GetMetaData(tmEntry, tmId);
                            lstTMMatches.Add(tmpTmEntry);
                        }
                    }

                    lstTMMatches = lstTMMatches.Take(maxHits).ToList();
                    return lstTMMatches.ToArray();
                }

                if (sourceText.Length < NGramLength)
                {
                    sourceText = sourceText.PadLeft(sourceText.Length + 1);
                    if (sourceText.Length < NGramLength)
                        sourceText = sourceText.PadRight(sourceText.Length + 1);
                    if (sourceText.Length < NGramLength)
                        sourceText = sourceText.PadLeft(NGramLength);
                }

                //search in source
                var loopCntr = 0;
                var scoreCntr = 0;
                var ROUGH_CUTOFF = 0.5;
                //we need the source as TextFragment
                foreach (var tmId in tmIds)
                {
                    //get the connector
                    var tmWriter = GetTMWriter(tmId);
                    var reader = tmWriter.IndexWriter.GetReader(false); // No benefit of OpenIfChanged

                    //the unique terms
                    var terms = CATUtils.GetTermsFromText(sourceText);
                    var uniqeTerms = new HashSet<string>(terms);

                    // initialize buffers
                    var maxDoc = reader.MaxDoc;
                    var scoredDocs = new short[maxDoc];
                    var docValues = new short[maxDoc];
                    var docPointers = new List<int>(); // new FixedBitSet(maxDoc) can be used.

                    string searchFieldName = TranslationUnitField.SOURCE.ToString();
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
                            quality = 2.0f * scoredDocs[docId]
                                    / (docValues[docId] + uniqueTermSize) * 100.0f;
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
                        var dsTMEntries = _dataStorage.GetTMEntriesBySourceIds(tmId, lstSourceIds.ToArray());
                        var aTMEntries = dsTMEntries.Tables[0].Rows;
                        foreach (DataRow tmEntry in aTMEntries)
                        {
                            var tmpTmEntry = new TMEntry();
                            var matchSourceCoded = (string)tmEntry["source"];
                            tmpTmEntry.id = (int)(long)tmEntry["id"];
                            tmpTmEntry.source = CATUtils.TextFragmentToTmx(new TextFragment(matchSourceCoded));
                            tmpTmEntry.target = CATUtils.TextFragmentToTmx(new TextFragment((string)tmEntry["target"]));
                            tmpTmEntry.metadata = GetMetaData(tmEntry, tmId);
                            lstTMMatches.Add(tmpTmEntry);
                        }
                    }
                }

                lstTMMatches = lstTMMatches.Take(maxHits).ToList();

                Debug.WriteLine("Loops:" + loopCntr + " scores: " + scoreCntr);

                return lstTMMatches.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// CalculateContextHashFromMetadata
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private int CalculateContextHashFromMetadata(Dictionary<string, string> metadata)
        {
            if (metadata.ContainsKey("contextHash"))
            {
                _ = int.TryParse(metadata["contextHash"], out int contextHash);
                return contextHash;
            }

            TextFragment prev = default!;
            TextFragment next = default!;
            if (metadata.ContainsKey("prevSegment"))
                prev = CATUtils.TmxSegmentToTextFragmentSimple(metadata["prevSegment"]);
            if (metadata.ContainsKey("nextSegment"))
                next = CATUtils.TmxSegmentToTextFragmentSimple(metadata["nextSegment"]);

            return CalculateContextHash(prev!, next!);
        }

        /// <summary>
        /// CalculateContextHash
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private int CalculateContextHash(TextFragment prev, TextFragment next)
        {
            string sContext = "";
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
        public int AddTMEntries(string tmId, TMEntry[] tmEntries)
        {
            try
            {
                int itemsAfter = 0;
                int itemsBefore = 0;
                //check the TM
                if (!TMExists(tmId))
                    throw new InvalidOperationException("The TM doesn't exist.");

                var transactionOptions = new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout = TransactionManager.MaximumTimeout
                };
                var tuTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
                using (tuTransaction)
                {
                    var tmWriter = GetTMWriter(tmId);
                    // get the number of entries before the import
                    itemsBefore = tmWriter.IndexWriter.NumDocs;

                    //import the entries
                    foreach (var tmEntry in tmEntries)
                    {
                        //convert the texts to text fragments
                        var source = CATUtils.TmxSegmentToTextFragmentSimple(tmEntry.source.Trim());
                        var target = CATUtils.TmxSegmentToTextFragmentSimple(tmEntry.target.Trim());
                        //the context
                        var metadata = JsonConvert.DeserializeObject<Dictionary<string, string>>(tmEntry.metadata);
                        var context = CalculateContextHashFromMetadata(metadata!);

                        string idUser = "";
                        metadata!.TryGetValue("user", out idUser!);
                        int speciality = 0;
                        if (metadata.ContainsKey("speciality"))
                            _ = int.TryParse(metadata["speciality"], out speciality);
                        int jobId = -1;
                        if (metadata.ContainsKey("jobId"))
                            _ = int.TryParse(metadata["jobId"], out jobId);
                        //insert into SQL server
                        var dsResult = _dataStorage.InsertTMEntry(tmId, source, target, context.ToString(), idUser!, speciality, jobId,
                            DateTime.Now, DateTime.Now, metadata.ContainsKey("metadata") ? metadata["metadata"] : "");
                        //var dsResult = _dataStorage.InsertTMEntry(tmId, source, target, context.ToString(), idUser, speciality, idTranslation,
                        //    DateTime.Now, DateTime.Now, ""); //[AM:29/09/2023] hotfix
                        var rowResult = dsResult.Tables[0].Rows[0];
                        var id = (int)(long)rowResult["sourceId"];
                        if (Convert.ToBoolean(rowResult["isNew"]))
                        {
                            //index the source text
                            tmWriter.IndexSource(id, source, speciality.ToString());
                        }
                        else if ((string)rowResult["oldSpecialities"] != (string)rowResult["newSpecialities"])
                        {
                            var oldSpecialities = string.Join(",", ((string)rowResult["oldSpecialities"]).Split(',').Distinct().OrderBy(element => element).ToArray());
                            var newSpecialities = string.Join(",", ((string)rowResult["newSpecialities"]).Split(',').Distinct().OrderBy(element => element).ToArray());
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

                return itemsAfter - itemsBefore;
            }
            catch (Exception ex)
            {
                _logger.LogError("TMEntries.log", "ERROR: AddTMEntries TM name -> " + tmId + "\n\n" + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// DeleteTMEntry
        /// </summary>
        /// <param name="tmId"></param>
        /// <param name="entryId"></param>
        public void DeleteTMEntry(string tmId, int entryId)
        {
            try
            {
                //check the TM
                if (!TMExists(tmId))
                    throw new InvalidOperationException("The TM doesn't exist.");

                var transactionOptions = new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout = TransactionManager.MaximumTimeout
                };
                var tuTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
                using (tuTransaction)
                {
                    //delete the entry from the SQL database
                    var luceneId = _dataStorage.DeleteTMEntry(tmId, entryId);
                    if (luceneId >= 0)
                    {
                        var tmWriter = GetTMWriter(tmId);
                        //delete the entry from the Lucene index
                        tmWriter.Delete(luceneId);
                        tmWriter.Commit();
                    }
                    tuTransaction.Complete();
                }

                //backup
            }
            catch (Exception ex)
            {
                _logger.LogError("TMEntries.log", "ERROR: DeleteTMEntry TM name -> " + tmId + "\n\n" + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// UpdateTMEntry
        /// </summary>
        /// <param name="tmId"></param>
        /// <param name="idEntry"></param>
        /// <param name="fieldsToUpdate"></param>
        public void UpdateTMEntry(string tmId, int idEntry, Dictionary<string, string> fieldsToUpdate)
        {
            try
            {
                var start = CATUtils.CurrentTimeMillis();
                //check the TM
                if (!TMExists(tmId))
                    throw new InvalidOperationException("The TM doesn't exist.");

                _dataStorage.UpdateTMEntry(tmId, idEntry, fieldsToUpdate);
                Debug.WriteLine("UpdateTMEntry: " + (CATUtils.CurrentTimeMillis() - start) + ".ms");
            }
            catch (Exception ex)
            {
                _logger.LogError("TMEntries.log", "ERROR: UpdateTMEntry, TM name -> " + tmId + "\n\n" + ex.ToString());
                throw;
            }
        }

        private string GetTwoLetterLangCode(string langCode)
        {
            //return new CultureInfo(sSourceLangIso639_1).TwoLetterISOLanguageName
            return langCode[..2].ToLower();
        }
        /// <summary>
        /// ImportTmx
        /// </summary>
        /// <param name="tmId"></param>
        /// <param name="sourceLangIso639_1"></param>
        /// <param name="targetLangIso639_1"></param>
        /// <param name="tmxContent"></param>
        /// <param name="user"></param>
        /// <param name="speciality"></param>
        /// <returns></returns>
        public TMImportResult ImportTmx(string tmId, string sourceLangIso639_1, string targetLangIso639_1, string tmxContent,
            string user, int speciality)
        {
            int cntr = 0;
            try
            {
                //check the TM
                if (!TMExists(tmId))
                    CreateTM(tmId);

                int itemsAfter = 0;
                int itemsBefore = 0;
                // read the entries from the tmx
                var tmx = new XmlDocument();
                tmx.LoadXml(tmxContent);
                var tus = tmx.GetElementsByTagName("tu");

                var transactionOptions = new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout = TransactionManager.MaximumTimeout
                };

                var tmWriter = GetTMWriter(tmId);
                itemsBefore = _dataStorage.GetTMEntriesNumber(tmId);

                //the TransactionManager.MaximumTimeout is 10 minutes. We need to use transaction blocks.
                var swTimeout = new Stopwatch();
                var from = 0;
                var srcTwoLetterISO = GetTwoLetterLangCode(sourceLangIso639_1);
                var targetTwoLetterISO = GetTwoLetterLangCode(targetLangIso639_1);
                string formatStringISO8601 = "yyyyMMdd'T'HHmmss'Z'";
                while (from < tus.Count)
                {
                    var bCommitted = false;
                    swTimeout.Start();
                    //the transaction scope
                    var tuTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
                    using (tuTransaction)
                    {
                        // get the number of entries before the import
                        for (int i = from; i < tus.Count; i++)
                        {
                            var tu = (XmlElement)tus[i]!;
                            var tuProps = tu.GetElementsByTagName("prop");
                            //the extension metadata
                            var metadataExtension = new Dictionary<string, string>();
                            //the dates
                            var dateCreated = DateTime.Now;
                            var dateModified = DateTime.Now;
                            if (tu.Attributes["creationdate"] != null)
                                dateCreated = DateTime.ParseExact(tu!.Attributes!["creationdate"]!.Value, formatStringISO8601, CultureInfo.InvariantCulture);
                            if (tu.Attributes["changedate"] != null)
                                dateModified = DateTime.ParseExact(tu!.Attributes!["changedate"]!.Value, formatStringISO8601, CultureInfo.InvariantCulture);
                            int idTranslation = -1;
                            var context = "0"; //default context
                            var tuSpeciality = speciality;
                            var contextData = new Dictionary<string, string>();
                            foreach (XmlNode tuProp in tuProps)
                            {
                                //context sometimes it is stored in the tuv node
                                if (tuProp!.Attributes!["type"]!.Value!.ToLower() == "x-context-pre")
                                {
                                    var type = "prevSegment";
                                    var value = tuProp.InnerText.Replace("<seg>", "").Replace("</seg>", "").Trim();

                                    if (contextData.ContainsKey(type))
                                        contextData[type] = value;
                                    else
                                        contextData.Add(type, value);
                                    continue;
                                }
                                else if (tuProp.Attributes["type"]!.Value.ToLower() == "x-context-post")
                                {
                                    var type = "nextSegment";
                                    var value = tuProp.InnerText.Replace("<seg>", "").Replace("</seg>", "").Trim();

                                    if (contextData.ContainsKey(type))
                                        contextData[type] = value;
                                    else
                                        contextData.Add(type, value);
                                    continue;

                                }
                                else if (tuProp.Attributes["type"]!.Value.ToLower() == "x-context")
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
                                if (tuProp.Attributes["type"]!.Value.ToLower() == "x-translation")
                                {
                                    if (!int.TryParse(tuProp.InnerText, out idTranslation))
                                        idTranslation = -1;
                                    continue;
                                }

                                //speciality
                                if (tuProp.Attributes["type"]!.Value.ToLower() == "domain")
                                {
                                    //find by value
                                    int foundSpeciality = _specialities.FirstOrDefault(x => x.Value == tuProp.InnerText.ToLower()).Key;

                                    if (foundSpeciality > 0)
                                        tuSpeciality = foundSpeciality;
                                    continue;
                                }
                                if (metadataExtension.ContainsKey(tuProp.Attributes["type"]!.Value))
                                {
                                    metadataExtension[tuProp.Attributes["type"]!.Value] = tuProp.InnerText;
                                    _logger.LogError("TMEntries.log", tuProp.Attributes["type"]!.Value.ToString() + " " + tuProp.InnerText);
                                }
                                else
                                    metadataExtension.Add(tuProp.Attributes["type"]!.Value, tuProp.InnerText);
                            }

                            var tuvs = tu.GetElementsByTagName("tuv");
                            // create a new entry
                            var tmEntry = new TMEntry();
                            bool bScrLangFound = false;
                            for (int j = 0; j < tuvs.Count; j++)
                            {
                                var tuv = (XmlElement)tuvs[j]!;
                                var lang = tuv.Attributes["xml:lang"]!.Value;
                                var langTwoLetterISO = GetTwoLetterLangCode(lang);
                                if (langTwoLetterISO == srcTwoLetterISO && !bScrLangFound)
                                { // does it need to be culture invariant?
                                    bScrLangFound = true;
                                    var segments = tuv.GetElementsByTagName("seg");
                                    if (segments.Count == 0)
                                        continue;
                                    tmEntry.source = segments[0]!.InnerXml.Trim();
                                    // set the context
                                    var props = tuv.GetElementsByTagName("prop");
                                    for (int k = 0; k < props.Count; k++)
                                    {
                                        var prop = props[k];
                                        var type = "";
                                        var typeAttr = prop!.Attributes!["type"]!.Value.ToLower();
                                        if (typeAttr == "x-context-pre")
                                            type = "prevSegment";
                                        else if (typeAttr == "x-context-post")
                                            type = "nextSegment";
                                        else if (typeAttr == "x-context")
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
                                    tmEntry.target = segments[0]!.InnerXml.Trim();
                                }
                                else
                                {
                                    Debug.WriteLine("Should never happen.");
                                }
                            }

                            // check if the entry is valid
                            if (!string.IsNullOrEmpty(tmEntry.source) && !string.IsNullOrEmpty(tmEntry.target))
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
                                    _logger.LogError("TMEntries.log", "ERROR: TM name -> " + tmId + "\nsource: " + tmEntry.source + "\ntargt: " +
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
                                var dsResult = _dataStorage.InsertTMEntry(tmId, source, target, context.ToString(), user, tuSpeciality, idTranslation,
                                    dateCreated, dateModified, extensionData);
                                var rowResult = dsResult.Tables[0].Rows[0];
                                var id = (int)(long)rowResult["sourceId"];
                                if (Convert.ToBoolean(rowResult["isNew"]))
                                {
                                    //index the source text
                                    tmWriter.IndexSource(id, source, tuSpeciality.ToString());
                                }
                                else if ((string)rowResult["oldSpecialities"] != (string)rowResult["newSpecialities"])
                                {
                                    var oldSpecialities = string.Join(",", ((string)rowResult["oldSpecialities"]).Split(',').Distinct().OrderBy(element => element).ToArray());
                                    var newSpecialities = string.Join(",", ((string)rowResult["newSpecialities"]).Split(',').Distinct().OrderBy(element => element).ToArray());
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
                                tmWriter = GetTMWriter(tmId); //keep alive
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
                            break;
                        }
                    }
                }

                //shrink the sql database
                ShrinkTM(tmId);
                //_dataStorage.shrinkDatabase(tmId)
                //tmWriter.IndexWriter.MaybeMerge()

                //the items after the import
                itemsAfter = _dataStorage.GetTMEntriesNumber(tmId);

                var ret = new TMImportResult
                {
                    allItems = itemsAfter,
                    importedItems = itemsAfter - itemsBefore
                };

                return ret;
            }
            catch (Exception ex)
            {
                _logger.LogError("TMEntries.log", "ERROR: TM name -> " + tmId + "\n\n" + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// GetTMList
        /// </summary>
        /// <returns></returns>
        public TMInfo[] GetTMList(bool bFullInfo)
        {
            var lstTMInfo = new List<TMInfo>();
            var aDirs = System.IO.Directory.GetDirectories(RepositoryFolder, "*", SearchOption.AllDirectories);
            foreach (string sDir in aDirs)
            {
                var tmDir = Path.GetFileName(sDir);
                if (tmDir.StartsWith('_'))
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
        public TMInfo[] GetTMListFromDatabase(string dbName, bool bFullInfo)
        {
            //get the TM tables from the database
            var dsTMNames = _dataStorage.GetTMListFromDatabase(dbName);
            var lstTMInfo = new List<TMInfo>();
            foreach (DataRow tmRow in dsTMNames.Tables[0].Rows)
            {
                var tmName = (string)tmRow["TABLE_NAME"];
                var tmInfo = GetTMInfo(dbName + "/" + tmName, bFullInfo);
                lstTMInfo.Add(tmInfo);
            }

            return lstTMInfo.ToArray();
        }

        public string ConnectionPoolInfo()
        {
            var activeWriters = new Dictionary<string, string>();
            //get the TM writers
            foreach (var key in TMConnectionPool.Keys)
            {
                string sTmWriter = "Created: " + TMConnectionPool[key].created.ToString() +
                    " Last access: " + TMConnectionPool[key].lastAccess.ToString();
                activeWriters.Add(key, sTmWriter);
            }

            return JsonConvert.SerializeObject(activeWriters);
        }
        /// <summary>
        /// GetTMWriter
        /// </summary>
        /// <param name="sTMName"></param>
        /// <returns></returns>
        private TMWriter GetTMWriter(string tmId)
        {
            lock (TMLock)
            {
                TMWriter tmWriter = default!;
                var sourceIndexDir = GetSourceIndexDirectory(tmId);
                // check if there is open connection
                if (TMConnectionPool.ContainsKey(tmId))
                {
                    var tmWriterContainer = TMConnectionPool[tmId];
                    tmWriterContainer.lastAccess = DateTime.Now;
                    tmWriter = tmWriterContainer.tmWriter;
                }
                else
                {
                    TMConnectionPool = TMConnectionPool.OrderBy(x => x.Value.lastAccess).ToDictionary(x => x.Key, x => x.Value);
                    var keysToRemove = new HashSet<string>();
                    //kick out the connectors that older than 20 minutes
                    foreach (var key in TMConnectionPool.Keys)
                    {
                        if (TMConnectionPool[key].lastAccess < DateTime.Now.AddMinutes(-1 * TMWriterIdleTimeout))
                            keysToRemove.Add(TMConnectionPool.Keys.First());
                    }
                    //kick out the oldest writer
                    if (TMConnectionPool.Count - keysToRemove.Count >= MaxTmConnectionPoolSize)
                        keysToRemove.Add(TMConnectionPool.Keys.First());

                    foreach (var key in keysToRemove)
                    {
                        try { TMConnectionPool[key].tmWriter.Close(); }
                        catch (Exception)
                        { //swallow the exception }
                            TMConnectionPool.Remove(key);
                        }

                        // create new connection
                        tmWriter = TmWriterFactory.CreateFileBasedTmWriter(sourceIndexDir, false);
                        var tmConnector = new TMConnector
                        {
                            id = tmId,
                            tmWriter = tmWriter,
                            created = DateTime.Now,
                            lastAccess = DateTime.Now
                        };

                        TMConnectionPool.Add(tmId, tmConnector);

                        //set database last access
                        _dataStorage.SetDatabaseLastAccess(GetDatabaseName(tmId));
                    }
                }

                return tmWriter;
            }
        }

        /// <summary>
        /// ReindexTM
        /// </summary>
        /// <param name="sTMName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int ReindexTM(string tmId, TMIndex index)
        {
            try
            {
                if (index == TMIndex.Source || index == TMIndex.Both) //the source index
                {
                    //get the data for the source
                    var dsIndexData = _dataStorage.GetSourceIndexData(tmId);
                    var indexData = dsIndexData.Tables[0].Rows;
                    //the TM writer
                    var sourceIndexDir = GetSourceIndexDirectory(tmId);
                    var tmRoot = GetTMDirectory(tmId);
                    if (!System.IO.Directory.Exists(tmRoot))
                        System.IO.Directory.CreateDirectory(tmRoot);
                    var indexDir = Path.Combine(tmRoot, "source indexes");
                    if (!System.IO.Directory.Exists(indexDir))
                        System.IO.Directory.CreateDirectory(indexDir);
                    var tmName = GetTMName(tmId);
                    var tmDir = Path.Combine(indexDir, tmName);
                    if (!System.IO.Directory.Exists(tmDir))
                        System.IO.Directory.CreateDirectory(tmDir);

                    //close the connection
                    if (TMConnectionPool.ContainsKey(tmId))
                    {
                        TMConnectionPool[tmId].tmWriter.Close();
                        TMConnectionPool.Remove(tmId);
                    }

                    var tmWriter = TmWriterFactory.CreateFileBasedTmWriter(sourceIndexDir, true);
                    tmWriter.Commit();

                    var sourceLookup = new Dictionary<int, dynamic>();
                    foreach (DataRow drSource in indexData)
                    {
                        var source = (string)drSource["source"];
                        var id = (int)drSource["idSource"];
                        var speciality = (int)drSource["speciality"];
                        if (sourceLookup.ContainsKey(id))
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
                        var specialities = string.Join(",", sourceData.specialities.ToArray());
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
                _logger.LogError("TMEntries.log", "ERROR: ReindexTM TM name -> " + tmId + "\n\n" + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ShrinkTM
        /// </summary>
        /// <param name="sTMName"></param>
        /// <returns></returns>
        public void ShrinkTM(string tmId)
        {
            try
            {
                //shrink the sql data
                var aPathElements = tmId.Split('/');
                var dbName = aPathElements[0];
                _dataStorage.ShrinkDatabase(dbName);

                //optimize the lucene index
                var tmWriter = GetTMWriter(tmId);
                tmWriter.IndexWriter.MaybeMerge();

            }
            catch (Exception ex)
            {
                _logger.LogError("TMEntries.log", "ERROR: shrinkDatabase TM name -> " + tmId + "\n\n" + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ExportTmx
        /// </summary>
        /// <param name="tmId"></param>
        /// <returns></returns>
        public string ExportTmx(string tmId)
        {
            try
            {
                //load the entries
                var dsEntries = _dataStorage.GetTranslationMemoryData(tmId);
                var drEntries = dsEntries.Tables[0].Rows;
                var tmInfo = GetTMInfo(tmId, false);
                var sbTmx = new StringBuilder();
                sbTmx.Append("<tmx version=\"1.4\">\r\n" + "<header creationtool=\"tm\" segtype=\"sentence\" srclang=\"" + tmInfo.langFrom +
                    "\" datatype=\"unknown\">\r\n");
                sbTmx.Append("<prop type=\"name\">" + tmId + "</prop>\r\n</header>\r\n\t");
                var sbTmxBody = new StringBuilder();
                foreach (DataRow drEntry in drEntries)
                {
                    var tu = "\r\n\t\t<tu tuid=\"" + (int)drEntry["id"] + "\" changedate=\"" + ((DateTime)drEntry["dateModified"]).ToString("yyyyMMdd'T'HHmmss'Z'") +
                        "\" creationdate=\"" +
                        ((DateTime)drEntry["dateCreated"]).ToString("yyyyMMdd'T'HHmmss'Z'") + "\" creationid=\"" + (string)drEntry["createdBy"] + "\" changeid=\"" + (string)drEntry["modifiedBy"] +
                        "\">\r\n{0}\r\n\t\t</tu>";
                    var sbTuContent = new StringBuilder();
                    //speciality
                    var idSpeciality = drEntry["speciality"] != DBNull.Value ? (int)drEntry["speciality"] : -1;
                    if (_specialities.ContainsKey(idSpeciality))
                        sbTuContent.Append("\t\t\t<prop type=\"domain\">" + _specialities[idSpeciality] + "</prop>\r\n");

                    int idTranslation = drEntry["idTranslation"] != DBNull.Value ? (int)drEntry["idTranslation"] : -1;
                    if (idTranslation > 0)
                        sbTuContent.Append("\t\t\t<prop type=\"x-translation\">" + idTranslation + "</prop>\r\n");

                    //the extension data
                    var sExtensionData = drEntry["extensionData"] != DBNull.Value ? (string)drEntry["extensionData"] : "";
                    if (sExtensionData != "")
                    {
                        try
                        {
                            var extensionData = JsonConvert.DeserializeObject<Dictionary<string, string>>(sExtensionData);
                            foreach (var prop in extensionData!.Keys)
                                sbTuContent.Append("\t\t\t<prop type=\"" + prop + "\">" + extensionData[prop] + "</prop>\r\n");
                        }
                        catch (Exception)
                        {
                            //swallow the exception
                        }
                    }

                    sbTuContent.Append("\t\t\t<tuv xml:lang=\"" + tmInfo.langFrom + "\">\r\n\t\t\t\t<prop type=\"x-context\">" + (string)drEntry["context"] +
                        "</prop>\r\n\t\t\t\t<seg>" + CATUtils.TextFragmentToTmx(new TextFragment((string)drEntry["source"])) + "</seg>\r\n\t\t\t</tuv>\r\n");
                    sbTuContent.Append("\t\t\t<tuv xml:lang=\"" + tmInfo.langTo + "\">\r\n\t\t\t\t<seg>" + CATUtils.TextFragmentToTmx(new TextFragment((string)drEntry["target"])) +
                        "</seg>\r\n\t\t\t</tuv>");
                    tu = string.Format(tu, sbTuContent.ToString());
                    sbTmxBody.Append(tu);
                }
                sbTmx.Append("<body>" + sbTmxBody + "\r\n\t</body>\r\n</tmx>");

                return sbTmx.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError("TMEntries.log", "ERROR: shrinkDatabase TM name -> " + tmId + "\n\n" + ex.ToString());
                throw;
            }
        }

        #region Misc.
        private string GetMetaData(DataRow tmEntry, string tmId)
        {
            try
            {
                var metadata = new Dictionary<string, string>
                {
                    { "origin", tmId },
                    { "dateCreated", tmEntry["dateCreated"] != DBNull.Value ? ((DateTime)tmEntry["dateCreated"]).ToString("M/d/yyyy h:mm:ss tt") : "N/A" },
                    { "createdBy", tmEntry["createdBy"] != DBNull.Value ? (string)tmEntry["createdBy"] : "N/A" },
                    { "dateModified", tmEntry["dateModified"] != DBNull.Value ? ((DateTime)tmEntry["dateModified"]).ToString("M/d/yyyy h:mm:ss tt") : "N/A" },
                    { "modifiedBy", tmEntry["modifiedBy"] != DBNull.Value ? (string)tmEntry["modifiedBy"] : "N/A" },
                    { "speciality", tmEntry["speciality"] != DBNull.Value ? ((int)tmEntry["speciality"]).ToString() : "N/A" },
                    { "idTranslation", tmEntry["idTranslation"] != DBNull.Value ? ((int)tmEntry["idTranslation"]).ToString() : "N/A" }
                };

                //the extension data
                try
                {
                    metadata.Add("extensionData", tmEntry["extensionData"] != DBNull.Value ? (string)tmEntry["extensionData"] : "");
                }
                catch (Exception)
                {
                    metadata.Add("extensionData", "");
                }

                var jsonMetadata = JsonConvert.SerializeObject(metadata);

                return jsonMetadata;
            }
            catch (Exception)
            {
                //_logger.LogError("Metadata.log", "Erros: " + ex.ToString())
            }

            return "";
        }

        [GeneratedRegex("_(.+)_(.+)")]
        private static partial Regex TMAttributesRegex();
        [GeneratedRegex("_sec_(.+)_(.+)_(.+)")]
        private static partial Regex SecondaryTMAttributesRegex();
        [GeneratedRegex("_(.+)_(.+)_(.+)")]
        private static partial Regex PrimaryTMAttributesRegex();
        #endregion
    }
}
