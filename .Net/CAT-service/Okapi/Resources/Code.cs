/*===========================================================================
  Copyright (C) 2008-2013 by the Okapi Framework contributors
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

namespace CAT.Okapi.Resources
{

    /**
	 * Represents an abstracted in-line code used in a TextFragment object.
	 * For example, a <code>&lt;b&gt;</code> tag in an HTML paragraph.
	 */
    public class Code
    {
        public static List<Code> StringToCodes(String data)
        {
            return new List<Code>();
            //throw new NotImplementedException();
        }

        public static String CodesToString(List<Code> codes, bool stripOuterData)
        {
            return "";
        }
    }
}