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

namespace okapi.resource
{

    /**
     * Provides methods for storing the content of a paragraph-type unit, to handle
     * its properties, annotations and segmentation.
     * <p>
     * The TextContainer is made of a collection of parts: Some are simple {@link TextPart} objects,
     * others are special {@link TextPart} objects called {@link Segment}.
     * <p>
     * A TextContainer has always at least one {@link Segment} part.
     */
    public class TextContainer
    {
        private Dictionary<String, String> properties;

        public TextFragment GetFirstContent()
        {
            throw new NotImplementedException();
        }

        public void  SetContent(TextFragment content)
        {
            throw new NotImplementedException();
        }

        public String GetProperty(String name)
        {
            if (properties != null && properties.ContainsKey(name))
                return properties[name];

            return null;
        }

        public void SetProperty(String name, String value)
        {
            if (properties == null)
                properties = new Dictionary<String, String>();
            properties.Add(name, value);
        }
    }
}
