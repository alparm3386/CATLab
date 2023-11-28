using Icu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CAT.Utils
{
    class SimpleWordcount
    {
        private Locale _locale;
        private BreakIterator _breakIterator;
        public SimpleWordcount(String localeId)
        {
            //_locale = new Icu.Locale(localeId);
            //_breakIterator = BreakIterator.CreateWordInstance(_locale);
        }

        /// <summary>
        /// Split
        /// </summary>
        /// <param name="type"></param>
        /// <param name="locale"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static String[] Split(BreakIterator.UBreakIteratorType type, Locale locale, String text)
        {
            var parts = BreakIterator.Split(BreakIterator.UBreakIteratorType.WORD, locale, text);

            return parts.ToArray();
        }

        /// <summary>
        /// CountWords
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public int CountWords(String text)
        {
            text = text.Replace("-", "");
            text = Regex.Replace(text, "\\d+\\.\\d+", "000");
            text = Regex.Replace(text, "'", "");
            text = Regex.Replace(text, "’", "");
            text = Regex.Replace(text, @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)", "URL");

            var matches = Regex.Matches(text, "\\w+");
            int totalWordCount = matches.Count;

            return totalWordCount;
            
            //int wordCount = 0;
            //using (_breakIterator)
            //{
            //    _breakIterator.SetText(text);
            //    int current = _breakIterator.MoveNext();
            //    while (true)
            //    {
            //        if (current == BreakIterator.DONE)
            //        {
            //            break;
            //        }
            //        // don't count various space and punctuation
            //        int status = _breakIterator.GetRuleStatus();
            //        if (status != (int)RuleBasedBreakIterator.UWordBreak.NONE)
            //        {
            //            wordCount++;
            //        }
            //        current = _breakIterator.MoveNext();
            //    }
            //}

            //return wordCount;
        }
    }
}
