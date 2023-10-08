namespace CAT.Models
{
    public class Statistics
    {
        public String sourceLang = default!;
        public String targetLang = default!;
        public int repetitions = 0;
        public int match_101 = 0;
        public int match_100 = 0;
        public int match_95_99 = 0;
        public int match_85_94 = 0;
        public int match_75_84 = 0;
        public int match_50_74 = 0;
        public int no_match = 0;
    }
}
