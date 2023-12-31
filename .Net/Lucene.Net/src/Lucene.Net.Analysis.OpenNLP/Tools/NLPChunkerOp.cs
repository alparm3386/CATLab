﻿// Lucene version compatibility level 8.2.0
using Lucene.Net.Support.Threading;
using opennlp.tools.chunker;


namespace Lucene.Net.Analysis.OpenNlp.Tools
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    /// <summary>
    /// Supply OpenNLP Chunking tool
    /// Requires binary models from OpenNLP project on SourceForge.
    /// </summary>
    public class NLPChunkerOp
    {
        private readonly ChunkerME chunker = null;

        public NLPChunkerOp(ChunkerModel chunkerModel) 
        {
            chunker = new ChunkerME(chunkerModel);
        }

        public virtual string[] GetChunks(string[] words, string[] tags, double[] probs)
        {
            UninterruptableMonitor.Enter(this);
            try
            {
                string[] chunks = chunker.chunk(words, tags);
                if (probs != null)
                    chunker.probs(probs);
                return chunks;
            }
            finally
            {
                UninterruptableMonitor.Exit(this);
            }
        }
    }
}
