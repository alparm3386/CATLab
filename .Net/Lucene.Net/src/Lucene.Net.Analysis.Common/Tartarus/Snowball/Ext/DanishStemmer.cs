﻿// Lucene version compatibility level 4.8.1
/*

Copyright (c) 2001, Dr Martin Porter
Copyright (c) 2002, Richard Boulton
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice,
    * this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
    * notice, this list of conditions and the following disclaimer in the
    * documentation and/or other materials provided with the distribution.
    * Neither the name of the copyright holders nor the names of its contributors
    * may be used to endorse or promote products derived from this software
    * without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

 */

using System.Text;

namespace Lucene.Net.Tartarus.Snowball.Ext
{
    /// <summary>
    /// This class was automatically generated by a Snowball to Java compiler
    /// It implements the stemming algorithm defined by a snowball script.
    /// </summary>
    public class DanishStemmer : SnowballProgram
    {
        // LUCENENET specific: Factored out methodObject by using Func<bool> instead of Reflection

        private readonly static Among[] a_0 = {
                    new Among ( "hed", -1, 1 ),
                    new Among ( "ethed", 0, 1 ),
                    new Among ( "ered", -1, 1 ),
                    new Among ( "e", -1, 1 ),
                    new Among ( "erede", 3, 1 ),
                    new Among ( "ende", 3, 1 ),
                    new Among ( "erende", 5, 1 ),
                    new Among ( "ene", 3, 1 ),
                    new Among ( "erne", 3, 1 ),
                    new Among ( "ere", 3, 1 ),
                    new Among ( "en", -1, 1 ),
                    new Among ( "heden", 10, 1 ),
                    new Among ( "eren", 10, 1 ),
                    new Among ( "er", -1, 1 ),
                    new Among ( "heder", 13, 1 ),
                    new Among ( "erer", 13, 1 ),
                    new Among ( "s", -1, 2 ),
                    new Among ( "heds", 16, 1 ),
                    new Among ( "es", 16, 1 ),
                    new Among ( "endes", 18, 1 ),
                    new Among ( "erendes", 19, 1 ),
                    new Among ( "enes", 18, 1 ),
                    new Among ( "ernes", 18, 1 ),
                    new Among ( "eres", 18, 1 ),
                    new Among ( "ens", 16, 1 ),
                    new Among ( "hedens", 24, 1 ),
                    new Among ( "erens", 24, 1 ),
                    new Among ( "ers", 16, 1 ),
                    new Among ( "ets", 16, 1 ),
                    new Among ( "erets", 28, 1 ),
                    new Among ( "et", -1, 1 ),
                    new Among ( "eret", 30, 1 )
                };

        private readonly static Among[] a_1 = {
                    new Among ( "gd", -1, -1 ),
                    new Among ( "dt", -1, -1 ),
                    new Among ( "gt", -1, -1 ),
                    new Among ( "kt", -1, -1 )
                };

        private readonly static Among[] a_2 = {
                    new Among ( "ig", -1, 1 ),
                    new Among ( "lig", 0, 1 ),
                    new Among ( "elig", 1, 1 ),
                    new Among ( "els", -1, 1 ),
                    new Among ( "l\u00F8st", -1, 2 )
                };

        private static readonly char[] g_v = { (char)17, (char)65, (char)16, (char)1, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)48, (char)0, (char)128 };

        private static readonly char[] g_s_ending = { (char)239, (char)254, (char)42, (char)3, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)16 };

        private int I_x;
        private int I_p1;
        private StringBuilder S_ch = new StringBuilder();

        private void copy_from(DanishStemmer other)
        {
            I_x = other.I_x;
            I_p1 = other.I_p1;
            S_ch = other.S_ch;
            base.CopyFrom(other);
        }

        private bool r_mark_regions()
        {
            int v_1;
            int v_2;
            // (, line 29
            I_p1 = m_limit;
            // test, line 33
            v_1 = m_cursor;
            // (, line 33
            // hop, line 33
            {
                int c = m_cursor + 3;
                if (0 > c || c > m_limit)
                {
                    return false;
                }
                m_cursor = c;
            }
            // setmark x, line 33
            I_x = m_cursor;
            m_cursor = v_1;
            // goto, line 34

            while (true)
            {
                v_2 = m_cursor;

                do
                {
                    if (!(InGrouping(g_v, 97, 248)))
                    {
                        goto lab1;
                    }
                    m_cursor = v_2;
                    goto golab0;
                } while (false);
                lab1:
                m_cursor = v_2;
                if (m_cursor >= m_limit)
                {
                    return false;
                }
                m_cursor++;
            }
            golab0:
            // gopast, line 34

            while (true)
            {

                do
                {
                    if (!(OutGrouping(g_v, 97, 248)))
                    {
                        goto lab3;
                    }
                    goto golab2;
                } while (false);
                lab3:
                if (m_cursor >= m_limit)
                {
                    return false;
                }
                m_cursor++;
            }
            golab2:
            // setmark p1, line 34
            I_p1 = m_cursor;
            // try, line 35

            do
            {
                // (, line 35
                if (!(I_p1 < I_x))
                {
                    goto lab4;
                }
                I_p1 = I_x;
            } while (false);
            lab4:
            return true;
        }

        private bool r_main_suffix()
        {
            int among_var;
            int v_1;
            int v_2;
            // (, line 40
            // setlimit, line 41
            v_1 = m_limit - m_cursor;
            // tomark, line 41
            if (m_cursor < I_p1)
            {
                return false;
            }
            m_cursor = I_p1;
            v_2 = m_limit_backward;
            m_limit_backward = m_cursor;
            m_cursor = m_limit - v_1;
            // (, line 41
            // [, line 41
            m_ket = m_cursor;
            // substring, line 41
            among_var = FindAmongB(a_0, 32);
            if (among_var == 0)
            {
                m_limit_backward = v_2;
                return false;
            }
            // ], line 41
            m_bra = m_cursor;
            m_limit_backward = v_2;
            switch (among_var)
            {
                case 0:
                    return false;
                case 1:
                    // (, line 48
                    // delete, line 48
                    SliceDel();
                    break;
                case 2:
                    // (, line 50
                    if (!(InGroupingB(g_s_ending, 97, 229)))
                    {
                        return false;
                    }
                    // delete, line 50
                    SliceDel();
                    break;
            }
            return true;
        }

        private bool r_consonant_pair()
        {
            int v_1;
            int v_2;
            int v_3;
            // (, line 54
            // test, line 55
            v_1 = m_limit - m_cursor;
            // (, line 55
            // setlimit, line 56
            v_2 = m_limit - m_cursor;
            // tomark, line 56
            if (m_cursor < I_p1)
            {
                return false;
            }
            m_cursor = I_p1;
            v_3 = m_limit_backward;
            m_limit_backward = m_cursor;
            m_cursor = m_limit - v_2;
            // (, line 56
            // [, line 56
            m_ket = m_cursor;
            // substring, line 56
            if (FindAmongB(a_1, 4) == 0)
            {
                m_limit_backward = v_3;
                return false;
            }
            // ], line 56
            m_bra = m_cursor;
            m_limit_backward = v_3;
            m_cursor = m_limit - v_1;
            // next, line 62
            if (m_cursor <= m_limit_backward)
            {
                return false;
            }
            m_cursor--;
            // ], line 62
            m_bra = m_cursor;
            // delete, line 62
            SliceDel();
            return true;
        }

        private bool r_other_suffix()
        {
            int among_var;
            int v_1;
            int v_2;
            int v_3;
            int v_4;
            // (, line 65
            // do, line 66
            v_1 = m_limit - m_cursor;

            do
            {
                // (, line 66
                // [, line 66
                m_ket = m_cursor;
                // literal, line 66
                if (!(Eq_S_B(2, "st")))
                {
                    goto lab0;
                }
                // ], line 66
                m_bra = m_cursor;
                // literal, line 66
                if (!(Eq_S_B(2, "ig")))
                {
                    goto lab0;
                }
                // delete, line 66
                SliceDel();
            } while (false);
            lab0:
            m_cursor = m_limit - v_1;
            // setlimit, line 67
            v_2 = m_limit - m_cursor;
            // tomark, line 67
            if (m_cursor < I_p1)
            {
                return false;
            }
            m_cursor = I_p1;
            v_3 = m_limit_backward;
            m_limit_backward = m_cursor;
            m_cursor = m_limit - v_2;
            // (, line 67
            // [, line 67
            m_ket = m_cursor;
            // substring, line 67
            among_var = FindAmongB(a_2, 5);
            if (among_var == 0)
            {
                m_limit_backward = v_3;
                return false;
            }
            // ], line 67
            m_bra = m_cursor;
            m_limit_backward = v_3;
            switch (among_var)
            {
                case 0:
                    return false;
                case 1:
                    // (, line 70
                    // delete, line 70
                    SliceDel();
                    // do, line 70
                    v_4 = m_limit - m_cursor;

                    do
                    {
                        // call consonant_pair, line 70
                        if (!r_consonant_pair())
                        {
                            goto lab1;
                        }
                    } while (false);
                    lab1:
                    m_cursor = m_limit - v_4;
                    break;
                case 2:
                    // (, line 72
                    // <-, line 72
                    SliceFrom("l\u00F8s");
                    break;
            }
            return true;
        }

        private bool r_undouble()
        {
            int v_1;
            int v_2;
            // (, line 75
            // setlimit, line 76
            v_1 = m_limit - m_cursor;
            // tomark, line 76
            if (m_cursor < I_p1)
            {
                return false;
            }
            m_cursor = I_p1;
            v_2 = m_limit_backward;
            m_limit_backward = m_cursor;
            m_cursor = m_limit - v_1;
            // (, line 76
            // [, line 76
            m_ket = m_cursor;
            if (!(OutGroupingB(g_v, 97, 248)))
            {
                m_limit_backward = v_2;
                return false;
            }
            // ], line 76
            m_bra = m_cursor;
            // -> ch, line 76
            S_ch = SliceTo(S_ch);
            m_limit_backward = v_2;
            // name ch, line 77
            if (!(Eq_V_B(S_ch.ToString())))
            {
                return false;
            }
            // delete, line 78
            SliceDel();
            return true;
        }


        public override bool Stem()
        {
            int v_1;
            int v_2;
            int v_3;
            int v_4;
            int v_5;
            // (, line 82
            // do, line 84
            v_1 = m_cursor;

            do
            {
                // call mark_regions, line 84
                if (!r_mark_regions())
                {
                    goto lab0;
                }
            } while (false);
            lab0:
            m_cursor = v_1;
            // backwards, line 85
            m_limit_backward = m_cursor; m_cursor = m_limit;
            // (, line 85
            // do, line 86
            v_2 = m_limit - m_cursor;

            do
            {
                // call main_suffix, line 86
                if (!r_main_suffix())
                {
                    goto lab1;
                }
            } while (false);
            lab1:
            m_cursor = m_limit - v_2;
            // do, line 87
            v_3 = m_limit - m_cursor;

            do
            {
                // call consonant_pair, line 87
                if (!r_consonant_pair())
                {
                    goto lab2;
                }
            } while (false);
            lab2:
            m_cursor = m_limit - v_3;
            // do, line 88
            v_4 = m_limit - m_cursor;

            do
            {
                // call other_suffix, line 88
                if (!r_other_suffix())
                {
                    goto lab3;
                }
            } while (false);
            lab3:
            m_cursor = m_limit - v_4;
            // do, line 89
            v_5 = m_limit - m_cursor;

            do
            {
                // call undouble, line 89
                if (!r_undouble())
                {
                    goto lab4;
                }
            } while (false);
            lab4:
            m_cursor = m_limit - v_5;
            m_cursor = m_limit_backward; return true;
        }
        public override bool Equals(object o)
        {
            return o is DanishStemmer;
        }

        public override int GetHashCode()
        {
            return this.GetType().FullName.GetHashCode();
        }
    }
}