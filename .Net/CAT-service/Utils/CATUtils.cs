using CAT.Okapi.analysis;
using CAT.Okapi.Resources;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Index;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace CAT.Utils
{
    class CATUtils
    {
        public static int djb2hash(String str)
        {
            int hash = 0;
            var chars = str.ToCharArray();
            for (int i = 0; i < str.Length; i++)
            {
                hash = chars[i] + ((hash << 5) - hash);
            }
            return hash;
        }

        public static TextFragment XliffSegmentToTextFragmentSimple(String sXliffSegment)
        {
            try
            {
                sXliffSegment ??= "";

                // remove the outer tag
                StringBuilder sbOut = new StringBuilder();

                // matcher for the tmx tags
                var matches = Regex.Matches(sXliffSegment, "<[^>]*>[^>]*>");
                int id = 1;
                int prevEnd = 0;
                var idStack = new Stack<int>();
                String sText = "";
                foreach (Match match in matches)
                {
                    String sTag = match.Value.ToLower();
                    sText = sXliffSegment.Substring(prevEnd, match.Index - prevEnd);
                    sText = HttpUtility.HtmlDecode(sText); //xml decode
                    prevEnd = match.Index + match.Length;
                    sbOut.Append(sText);
                    if (sTag.StartsWith("<bpt"))
                    {
                        sbOut.Append("" + ((char)TextFragment.MARKER_OPENING) + (char)(TextFragment.CHARBASE + id));
                        idStack.Push(id);
                        id++;
                    }
                    else if (sTag.StartsWith("<ept"))
                    {
                        sbOut.Append("" + ((char)TextFragment.MARKER_CLOSING) + (char)(TextFragment.CHARBASE + idStack.Pop()));
                    }
                    else if (sTag.StartsWith("<ph") || sTag.StartsWith("<x"))
                    {
                        sbOut.Append("" + ((char)TextFragment.MARKER_ISOLATED) + (char)(TextFragment.CHARBASE + id));
                        id++;
                    }
                }

                sText = sXliffSegment.Substring(prevEnd, sXliffSegment.Length - prevEnd);
                sText = HttpUtility.HtmlDecode(sText); //xml decode
                sbOut.Append(sText);

                return new TextFragment(sbOut.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("XliffSegmentToTextFragmentSimple: " + ex.Message);
            }
            finally
            {
            }
        }

        public static String TextFragmentToXliff(TextFragment fragment)
        {
            //create simple codes
            StringBuilder tmp = new StringBuilder();
            var codedText = fragment.GetCodedText();
            for (int i = 0; i < codedText.Length; i++)
            {
                var charCode = codedText[i];
                if (charCode == TextFragment.MARKER_OPENING)
                    tmp.Append("<bpt id=\"" + ((int)codedText[++i] - TextFragment.CHARBASE).ToString() + "\">{}</bpt>");
                else if (charCode == TextFragment.MARKER_CLOSING)
                    tmp.Append("<ept id=\"" + ((int)codedText[++i] - TextFragment.CHARBASE).ToString() + "\">{}</ept>");
                else if (charCode == TextFragment.MARKER_ISOLATED)
                    tmp.Append("<ph id=\"" + ((int)codedText[++i] - TextFragment.CHARBASE).ToString() + "\">{}</ph>");
                else
                {
                    //xml escape 
                    switch (charCode)
                    {
                        case '>': tmp.Append("&gt;"); break;
                        case '<': tmp.Append("&lt;"); break;
                        case '\r': tmp.Append("&#13;"); break; // Not a line-break in the XML context, but a literal
                        case '&': tmp.Append("&amp;"); break;
                        //case '"': tmp.Append("&quot;";
                        //case '\'': tmp.Append("&apos;"; //"&#39;"
                        default:
                            tmp.Append(charCode); break;
                    }
                }
            }

            return tmp.ToString();
        }

        public static String TextFragmentToTmx(TextFragment fragment)
        {
            //create simple codes
            StringBuilder tmp = new StringBuilder();
            var codedText = fragment.GetCodedText();
            for (int i = 0; i < codedText.Length; i++)
            {
                var charCode = codedText[i];
                if (charCode == TextFragment.MARKER_OPENING)
                    tmp.Append("<bpt i=\"" + ((int)codedText[++i] - TextFragment.CHARBASE).ToString() + "\">{}</bpt>");
                else if (charCode == TextFragment.MARKER_CLOSING)
                    tmp.Append("<ept i=\"" + ((int)codedText[++i] - TextFragment.CHARBASE).ToString() + "\">{}</ept>");
                else if (charCode == TextFragment.MARKER_ISOLATED)
                    tmp.Append("<ph i=\"" + ((int)codedText[++i] - TextFragment.CHARBASE).ToString() + "\">{}</ph>");
                else
                {
                    //xml escape 
                    switch (charCode)
                    {
                        case '>': tmp.Append("&gt;"); break;
                        case '<': tmp.Append("&lt;"); break;
                        case '\r': tmp.Append("&#13;"); break; // Not a line-break in the XML context, but a literal
                        case '&': tmp.Append("&amp;"); break;
                        //case '"': tmp.Append("&quot;";
                        //case '\'': tmp.Append("&apos;"; //"&#39;"
                        default:
                            tmp.Append(charCode); break;
                    }
                }
            }

            return tmp.ToString();
        }

        public static TextFragment TmxSegmentToTextFragmentSimple(String sTmxSeg)
        {
            try
            {
                if (String.IsNullOrEmpty(sTmxSeg))
                    return new TextFragment();

                // remove the outer tag
                StringBuilder sbOut = new StringBuilder();

                // matcher for the tmx tags
                var matches = Regex.Matches(sTmxSeg, "<[^>]*>[^>]*>");
                int id = 1;
                int prevEnd = 0;
                var idStack = new Stack<int>();
                String sText = "";
                foreach (Match match in matches)
                {
                    String sTag = match.Value.ToLower();
                    sText = sTmxSeg.Substring(prevEnd, match.Index - prevEnd);
                    sText = HttpUtility.HtmlDecode(sText); //xml decode
                    prevEnd = match.Index + match.Length;
                    sbOut.Append(sText);
                    if (sTag.StartsWith("<bpt"))
                    {
                        sbOut.Append("" + ((char)TextFragment.MARKER_OPENING) + (char)(TextFragment.CHARBASE + id));
                        idStack.Push(id);
                        id++;
                    }
                    else if (sTag.StartsWith("<ept"))
                    {
                        sbOut.Append("" + ((char)TextFragment.MARKER_CLOSING) + (char)(TextFragment.CHARBASE + idStack.Pop()));
                    }
                    else if (sTag.StartsWith("<ph") || sTag.StartsWith("<x") || sTag.StartsWith("<it"))
                    {
                        sbOut.Append("" + ((char)TextFragment.MARKER_ISOLATED) + (char)(TextFragment.CHARBASE + id));
                        id++;
                    }
                }

                sText = sTmxSeg.Substring(prevEnd, sTmxSeg.Length - prevEnd);
                sText = HttpUtility.HtmlDecode(sText); //xml decode
                sbOut.Append(sText);

                return new TextFragment(sbOut.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("XliffSegmentToTextFragmentSimple: " + ex.Message);
            }
            finally
            {
            }
        }
        public static List<String> GetTermsFromText(String text)
        {
            try
            {
                const int NGramLength = 4;
                //the short length text
                if (text.Length <= NGramLength)
                {
                    text = text.Replace("  ", " ");
                    text = text.PadRight(NGramLength, '$');
                }

                // the analysis
                NgramAnalyzer defaultFuzzyAnalyzer = new NgramAnalyzer(4);

                // create basic ngram analyzer to tokenize query
                var queryTokenStream = defaultFuzzyAnalyzer.GetTokenStream("SOURCE", text);
                var termAtt = queryTokenStream.AddAttribute<ICharTermAttribute>();

                var terms = new List<String>();
                queryTokenStream.Reset();
                while (queryTokenStream.IncrementToken())
                {
                    terms.Add(termAtt.ToString());
                }
                queryTokenStream.Dispose();
                return terms;
            }
            catch (IOException ex)
            {
                // TODO Auto-generated catch block
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// CurrentTimeMillis
        /// </summary>
        /// <returns></returns>
        public static long CurrentTimeMillis()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// SanitizeFileName
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String SanitizeFileName(String name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }
}
