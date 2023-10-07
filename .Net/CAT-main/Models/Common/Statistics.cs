namespace CAT.Models.Common
{
    /// <summary>
    /// Statistics
    /// </summary>
    [Serializable()]
    public class Statistics
    {
        public int targetLang;
        public int repetitions = 0;
        public int match_100 = 0;
        public int match_101 = 0;
        public int match_50_74 = 0;
        public int match_75_84 = 0;
        public int match_85_94 = 0;
        public int match_95_99 = 0;
        public int no_match = 0;
        public int WordCount
        {
            get { return repetitions + match_100 + match_101 + match_50_74 + match_75_84 + match_85_94 + match_95_99 + no_match; }
        }

        public void Add(Statistics stats)
        {
            repetitions += stats.repetitions;
            match_100 += stats.match_100;
            match_101 += stats.match_101;
            match_50_74 += stats.match_50_74;
            match_75_84 += stats.match_75_84;
            match_85_94 += stats.match_85_94;
            match_95_99 += stats.match_95_99;
            no_match += stats.no_match;
        }

        public bool ExactMatchesOnly()
        {
            return no_match == 0 && match_50_74 == 0 && match_75_84 == 0
                        && match_85_94 == 0 && match_95_99 == 0 && (match_100 != 0 || match_101 != 0);
        }

        public static Statistics[] GetStatisticsByWordCount(int nWordCount, int targetLang)
        {
            return new Statistics[] { new Statistics() { no_match = nWordCount,
                    targetLang = targetLang} };
        }
    }

}
