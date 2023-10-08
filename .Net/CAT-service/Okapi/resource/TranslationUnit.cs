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
using System.Collections.Generic;

namespace okapi.resource
{

    /**
	 * Represents a Unit of Translation.
	 */
    public class TranslationUnit
    {
        public int tuid;
        public TextFragment source;
        public TextFragment target;
        public String context;

        /**
		 * Creates a TU w/o an source or target defined
		 */
        public TranslationUnit()
        {
        }

        /**
		 * Creates a TU with the provided source and targets
		 * 
		 * @param source
		 *            The source of the TU
		 * @param target
		 *            The target of the TU
		 */
        public TranslationUnit(TextFragment source, TextFragment target, String context) : this()
        {
            this.source = source;
            this.target = target;
            this.context = context;
        }

        /**
		 * Checks to see if the the source is empty
		 * 
		 * @return true if the source is empty
		 */
        public bool IsSourceEmpty()
        {
            return IsFragmentEmpty(source);
        }

        /**
		 * Checks to see if the the target is empty
		 * 
		 * @return true if the target is empty
		 */
        public bool IsTargetEmpty()
        {
            return IsFragmentEmpty(target);
        }

        private static bool IsFragmentEmpty(TextFragment frag)
        {
            return (frag == null || String.IsNullOrEmpty(frag.GetCodedText()));
        }

        public override String ToString()
        {
            return "Source: " + source.ToText() + "\nTarget: " + target.ToText();

        }
    }
}
