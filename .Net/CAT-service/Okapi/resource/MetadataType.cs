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
using System.ComponentModel;

namespace okapi.resource
{
    /**
     * The types of metadata that is supported. Currently all properties use the same store and indexTypes
     * @author HaslamJD
     */
    public enum MetadataType
    {
        [Description("Context")]
        CONTEXT = 1,
        [Description("DateCreated")]
        DATE_CREATED = 2,
        [Description("CretedBy")]
        CREATED_BY = 3,
        [Description("DateModified")]
        DATE_MODIFIED = 4,
        [Description("ModifiedBy")]
        MODIFIED_BY = 5,
        [Description("Speciality")]
        SPECIALITY = 6
    }
}
