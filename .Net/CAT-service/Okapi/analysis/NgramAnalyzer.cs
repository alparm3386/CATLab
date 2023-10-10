/*===========================================================================
Copyright (C) 2008-2009 by the Okapi Framework contributors
-----------------------------------------------------------------------------
Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
===========================================================================*/

/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Pattern;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
/**
* 
* @author HaslamJD
* @author HARGRAVEJE
*/
namespace CAT.Okapi.analysis
{
    public sealed class NgramAnalyzer : Analyzer
    {
        public static readonly int MaxInputSize = 4096;

        private int ngramLength;

        public NgramAnalyzer(int ngramLength)
        {
            if (ngramLength <= 0)
            {
                throw new ArgumentException(
                        "'ngramLength' cannot be less than 0");
            }
            this.ngramLength = ngramLength;
        }

        protected override TokenStreamComponents CreateComponents(String fieldName, TextReader reader)
        {
            /* Effectively implementing the analysis chain that would be
               written like this in Solr (where ngramLength = 4):
                 <analyzer>
                    <tokenizer class="solr.KeywordTokenizerFactory" maxTokenLen="4096"/>
                    <filter class="solr.PatternReplaceFilterFactory" pattern="(\s+)" replacement=" "/>
                    <filter class="solr.LowerCaseFilterFactory" />
                    <filter class="solr.EdgeNGramFilterFactory" minGramSize="4" maxGramSize="4" preserveOriginal="true"/>
                    <filter class="solr.LengthFilterFactory" max="4"/>
                </analyzer>
             */
            Tokenizer source = new KeywordTokenizer(reader, MaxInputSize);
            var patternReplaceFilter = new PatternReplaceFilter(source, new Regex("\\s+"), " ", true);
            var lowerCaseFilter = new LowerCaseFilter(Lucene.Net.Util.LuceneVersion.LUCENE_48, patternReplaceFilter);
            var ngramTokenFilter = new NGramTokenFilter( Lucene.Net.Util.LuceneVersion.LUCENE_48, lowerCaseFilter, ngramLength, ngramLength);

            TokenStream result = new LengthFilter(Lucene.Net.Util.LuceneVersion.LUCENE_48, ngramTokenFilter, 1, ngramLength);
            return new TokenStreamComponents(source, result);
        }
    }
}
