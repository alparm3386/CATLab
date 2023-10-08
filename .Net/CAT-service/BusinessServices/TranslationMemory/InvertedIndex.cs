using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using okapi.search.analysis;
using utils;

namespace cat.tm
{
    public class InvertedIndex
    {
        class IndexElement
        {
            public int id;
            public int termsNum;
            public String text;
        }

        private readonly static int N_GRAM_LEN = 4;
        private readonly static NgramAnalyzer defaultFuzzyAnalyzer = new NgramAnalyzer(N_GRAM_LEN);
        private Dictionary<String, List<int>> invertedIndex = new Dictionary<String, List<int>>();
        private List<IndexElement> indexElements = new List<IndexElement>();
        private int elementIdx = 0;

        public short[] scoredDocs = null;

        public InvertedIndex()
        {
        }

        public float GetHighestScore(String text, int threshold)
        {
            // extract the terms
            List<String> terms = GetUniqueTerms(text);

            // get the match candidates
            scoredDocs = new short[indexElements.Count];
            HashSet<String> uniqueTerms = new HashSet<String>(terms);
            List<int> matchCandidates = new List<int>();
            int uniqueTermsSize = uniqueTerms.Count;
            int roughCutoff = uniqueTermsSize / 2;
            foreach (String term in uniqueTerms)
            {
                List<int> lstDocIds;
                invertedIndex.TryGetValue(term, out lstDocIds);

                if (lstDocIds != null)
                {
                    foreach (int idDoc in lstDocIds)
                    {
                        var matches = ++scoredDocs[idDoc];
                        if (matches > roughCutoff)
                            matchCandidates.Add(idDoc);
                    }
                }
            }

            // do the scoring
            float maxScore = 0.0f;
            foreach (int idDoc in matchCandidates)
            {
                float score = 0;
                int matchedTermsNum = scoredDocs[idDoc];
                var indexElement = indexElements[idDoc];
                int termsNum = indexElement.termsNum;
                score = (2.0f * matchedTermsNum) / (termsNum + uniqueTerms.Count) * 100.0f;
                if (score >= 100)
                {
                    //it takes the capitalization and the tags into account
                    if (indexElement.text != (text))
                        score -= 2;
                }

                if (score > maxScore)
                    maxScore = score;
            }

            // the threshold
            if (maxScore < threshold)
                maxScore = 0;

            return maxScore;
        }

        public List<String> GetUniqueTerms(String text)
        {
            try
            {
                var terms = CATUtils.GetTermsFromText(text);
                return new HashSet<String>(terms).ToList();
            }
            catch (IOException ex)
            {
                // TODO Auto-generated catch block
                throw ex;
            }
        }

        public List<String> GetUniqueTermsFromIndex()
        {
            return invertedIndex.Keys.ToList();
        }

        public void Reset()
        {
            invertedIndex.Clear();
            indexElements.Clear();
            elementIdx = 0;
        }

        public void AddToIndex(String text)
        {
            // extract the terms
            List<String> terms = GetUniqueTerms(text);

            // the index element
            IndexElement indexElement = new IndexElement();
            indexElement.id = elementIdx;
            indexElement.termsNum = terms.Count;
            indexElement.text = text;
            elementIdx++;
            // do the indexing
            foreach (String term in terms)
            {
                List<int> docIds;
                invertedIndex.TryGetValue(term, out docIds!);
                if (docIds == null)
                {
                    List<int> lstIds = new List<int>();
                    lstIds.Add(indexElement.id);
                    invertedIndex.Add(term, lstIds);
                }
                else
                {
                    docIds.Add(indexElement.id);
                }
            }

            indexElements.Add(indexElement);
        }
    }
}