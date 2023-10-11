﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT_service_test.Utils
{
    public class SkipClassAttribute : Attribute
    {
        public string Reason { get; }

        public SkipClassAttribute(string reason)
        {
            Reason = reason;
        }
    }
}
