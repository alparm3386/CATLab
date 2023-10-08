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

using System;

namespace okapi.resource
{
    /**
	 * Represents a TM Hit. This stores a reference to the TranslationUnit and its
	 * score and {@link MatchType}
	 * 
	 * @author HaslamJD
	 * @author HARGRAVEJE
	 */
    public class TmHit : IComparable<TmHit>
    {

        private TranslationUnit tu;
        private float score;
        private bool codeMismatch;

        /**
         * Default constructor which sets the MatchType to NONE. 
         */
        public TmHit()
        {
            SetCodeMismatch(false);
        }

        /**
         * Create a new TmHit.
         * @param tu
         * @param matchType
         * @param score
         */
        public TmHit(TranslationUnit tu, float score)
        {
            this.tu = tu;
            this.score = score;
            SetCodeMismatch(false);
        }

        /**
         * TmHit's score. 
         */
        public float Score
        {
            get { return score; }
            set { score = value;  }
        }

        /**
         * TmHit's {@link TranslationUnit}
         */
        public TranslationUnit Tu
        {
            get { return tu; }
            set { tu = value; }
        }

        /**
         * Set true of the {@link Code}s between the TmHit and query {@link TextFragment} are different.
         * @param codeMismatch
         */
        public void SetCodeMismatch(bool codeMismatch)
        {
            this.codeMismatch = codeMismatch;
        }

        /**
         * Is there a difference between the {@link Code}s of the TmHit and the query {@link TextFragment}?  
         * @return true if there is a code difference.
         */
        public bool IsCodeMismatch()
        {
            return codeMismatch;
        }

        /**
         * This method implements a three way sort on (1) score (2)
         * source string. Score is the primary key, source secondary.
         * 
         * @param other - the TmHit we are comparing against.
         */
        public int CompareTo(TmHit other)
        {
            const int EQUAL = 0;

            if (this == other)
                return EQUAL;

            String thisSource = this.tu.Source.GetCodedText();
            String otherSource = other.tu.Source.GetCodedText();

            // only sort by match type if this or other is some kind of exact match
            int comparison;

            // compare score
            comparison = score.CompareTo(other.score);
            if (comparison != EQUAL)
                return comparison * -1;  // we want to reverse the normal score sort

            // compare source strings with codes
            comparison = thisSource.CompareTo(otherSource);
            if (comparison != EQUAL)
                return comparison;

            // default
            return EQUAL;
        }

        /**
         * Define equality of state.
         */
        public override bool Equals(Object other)
        {
            if (this == other)
                return true;
            if (!(other is TmHit))
                return false;

            TmHit otherHit = (TmHit)other;
            return (this.tu.Source.GetCodedText() == otherHit.tu.Source.GetCodedText());
        }

        /**
         * A class that overrides equals must also override hashCode.
         */
        public override int GetHashCode()
        {
            int result = 1;// (int)DateTime.Now.Ticks;
            result = tu.Source.GetCodedText().GetHashCode() * result;
            result = tu.Target.GetCodedText().GetHashCode() * result;

            return result;
        }
    }
}