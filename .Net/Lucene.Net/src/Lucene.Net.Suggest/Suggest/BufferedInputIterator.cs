﻿using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using JCG = J2N.Collections.Generic;

namespace Lucene.Net.Search.Suggest
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
    /// This wrapper buffers incoming elements.
    /// <para/>
    /// @lucene.experimental
    /// </summary>
    public class BufferedInputEnumerator : IInputEnumerator
    {
        // TODO keep this for now
        /// <summary>
        /// buffered term entries </summary>
        protected BytesRefArray m_entries = new BytesRefArray(Counter.NewCounter());
        /// <summary>
        /// buffered payload entries </summary>
        protected BytesRefArray m_payloads = new BytesRefArray(Counter.NewCounter());
        /// <summary>
        /// buffered context set entries </summary>
        protected IList<ICollection<BytesRef>> m_contextSets = new JCG.List<ICollection<BytesRef>>();
        /// <summary>
        /// current buffer position </summary>
        protected int m_curPos = -1;
        /// <summary>
        /// buffered weights, parallel with <see cref="m_entries"/> </summary>
        protected long[] m_freqs = new long[1];
        private readonly BytesRef spare = new BytesRef();
        private readonly BytesRef payloadSpare = new BytesRef();
        private readonly bool hasPayloads;
        private readonly IComparer<BytesRef> comp;

        private readonly bool hasContexts;
        protected BytesRef m_current;

        /// <summary>
        /// Creates a new iterator, buffering entries from the specified iterator </summary>
        public BufferedInputEnumerator(IInputEnumerator source)
        {
            int freqIndex = 0;
            hasPayloads = source.HasPayloads;
            hasContexts = source.HasContexts;
            while (source.MoveNext())
            {
                m_entries.Append(source.Current);
                if (hasPayloads)
                {
                    m_payloads.Append(source.Payload);
                }
                if (hasContexts)
                {
                    m_contextSets.Add(source.Contexts);
                }
                if (freqIndex >= m_freqs.Length)
                {
                    m_freqs = ArrayUtil.Grow(m_freqs, m_freqs.Length + 1);
                }
                m_freqs[freqIndex++] = source.Weight;
            }
            comp = source.Comparer;
        }

        public virtual long Weight => m_freqs[m_curPos];

        public virtual BytesRef Current => m_current;

        public virtual bool MoveNext()
        {
            if (++m_curPos < m_entries.Length)
            {
                m_entries.Get(spare, m_curPos);
                m_current = spare;
            }
            m_current = null;
            return false;
        }

        public virtual BytesRef Payload
        {
            get
            {
                if (hasPayloads && m_curPos < m_payloads.Length)
                {
                    return m_payloads.Get(payloadSpare, m_curPos);
                }
                return null;
            }
        }

        public virtual bool HasPayloads => hasPayloads;

        public virtual IComparer<BytesRef> Comparer => comp;

        public virtual ICollection<BytesRef> Contexts
        {
            get
            {
                if (hasContexts && m_curPos < m_contextSets.Count)
                {
                    return m_contextSets[m_curPos];
                }
                return null;
            }
        }

        public virtual bool HasContexts => hasContexts;
    }
}