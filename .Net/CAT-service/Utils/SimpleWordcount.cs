using Icu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CAT.Utils
{
    partial class SimpleWordcount
    {
        public SimpleWordcount(string localeId)
        {
        }

        /// <summary>
        /// Split
        /// </summary>
        /// <param name="type"></param>
        /// <param name="locale"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string[] Split(BreakIterator.UBreakIteratorType type, Locale locale, string text)
        {
            var parts = BreakIterator.Split(BreakIterator.UBreakIteratorType.WORD, locale, text);

            return parts.ToArray();
        }

        /// <summary>
        /// CountWords
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public int CountWords(string text)
        {
            var matches = WordCountRegex().Matches(text);
            int totalWordCount = matches.Count;

            return totalWordCount;
        }

        [GeneratedRegex("\\w+")]
        private static partial Regex WordCountRegex();
    }
}
