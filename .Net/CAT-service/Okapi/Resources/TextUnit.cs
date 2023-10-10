/*===========================================================================
  Copyright (C) 2008-2011 by the Okapi Framework contributors
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
     * Basic unit of extraction from a filter and also the resource associated with
     * the filter event TEXT_UNIT.
     * The TextUnit object holds the extracted source text in one or more versions,
     * all its properties and annotations, and any target corresponding data.
     */
    public class TextUnit
    {
        private TextContainer source = default!;
        private String tuid = default!;
        private String name = default!;
        private Dictionary<String, TextContainer> targets = new Dictionary<String, TextContainer>();
        private Dictionary<String, String> properties = default!;

        public TextUnit(String tuid)
        {
            this.tuid = tuid;
        }

        public TextContainer Source
        {
            get { return source; }
            set { source = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public Dictionary<String, TextContainer> Targets
        {
            get { return targets; }
        }

        public TextFragment SetSourceContent(TextFragment content)
        {
            source.SetContent(content);
            return source.GetFirstContent();
        }

        public TextFragment SetTargetContent(String locale, TextFragment content)
        {
            throw new NotImplementedException();
        }

        public String[] GetPropertyNames()
        {
            throw new NotImplementedException();
            //return properties.Keys.ToArray();
        }

        public String GetProperty(String name)
        {
            if (properties != null && properties.ContainsKey(name))
                return properties[name];

            return null!;
        }

        public void SetProperty(String name, String value)
        {
            properties ??= new Dictionary<String, String>();
            properties.Add(name, value);
        }
    }
}
