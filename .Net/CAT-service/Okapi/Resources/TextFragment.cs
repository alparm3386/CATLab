/*===========================================================================
  Copyright (C) 2008-2018 by the Okapi Framework contributors
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
using System.Text;
using System.Text.RegularExpressions;

namespace CAT.Okapi.Resources
{

	/**
	 * Implements the methods for creating and manipulating a pre-parsed
	 * flat representation of a content with in-line codes.
	 * 
	 * <p>The model uses two objects to store the data:
	 * <ul><li>a coded text string
	 * <li>a list of {@link Code} object.</ul>
	 * 
	 * <p>The coded text string is composed of normal characters and <b>markers</b>.
	 * 
	 * <p>A marker is a sequence of two special characters (in the Unicode PUA)
	 * that indicate the type of underlying code (opening, closing, isolated), and an index
	 * pointing to its corresponding Code object where more information can be found.
	 * The value of the index is encoded as a Unicode PUA character. You can use the
	 * {@link #toChar(int)} and {@link #toIndex(char)} methods to encoded and decode
	 * the index value.
	 * 
	 * <p>To get the coded text of a TextFragment object use {@link #getCodedText()}, and
	 * to get its list of codes use {@link #getCodes()}.
	 * 
	 * <p>You can modify directly the coded text or the codes and re-apply them to the
	 * TextFragment object using {@link #setCodedText(String)} and
	 * {@link #setCodedText(String, List)}.
	 *
	 * <p>Adding a code to the coded text can be done by:
	 * <ul><li>appending the code with {@link #append(TagType, String, String)}
	 * <li>changing a section of existing text to code with
	 * {@link #changeToCode(int, int, TagType, String)}</ul>
	 */
	public class TextFragment
	{
		/**
		 * Special character marker for a opening inline code.
		 */
		public static readonly int MARKER_OPENING = 0xE101;

		/**
		 * Special character marker for a closing inline code.
		 */
		public static readonly int MARKER_CLOSING = 0xE102;

		/**
		 * Special character marker for an isolated inline code.
		 */
		public static readonly int MARKER_ISOLATED = 0xE103;

		/**
		 * Special value used as the base of inline code indices. 
		 */
		public static readonly int CHARBASE = 0xE110;

		//regex for all TextFragment markers
		public static readonly String MARKERS_REGEX = "[\uE101\uE102\uE103\uE104].";

		private StringBuilder text;

		public TextFragment()
		{
			text = new StringBuilder();
			//isBalanced = true;
		}

		public TextFragment(String text)
		{
			this.text = new StringBuilder((text == null) ? "" : text);
		}

		public String ToText()
		{
			return text.ToString();
		}

		public static String GetText(String codedText)
		{
			String text = Regex.Replace(codedText, MARKERS_REGEX, "");
			return text;
		}

		public List<Code> GetCodes()
		{
			return new List<Code>();
		}

		public static List<Code> GetCodes(String codedText)
		{
			return new List<Code>();
		}

		public String GetText()
		{
			String plainText = Regex.Replace(text.ToString(), MARKERS_REGEX, "");
			return plainText;
		}

		public String GetCodedText()
		{
			return text.ToString();
		}

		public void SetCodedText(String newCodedText, List<Code> newCodes, bool allowCodeDeletion)
		{
			text = new StringBuilder(newCodedText);
		}
	}
}
