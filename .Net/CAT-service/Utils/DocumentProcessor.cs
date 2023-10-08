using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATService.Utils
{
    class DocumentProcessor
    {
        /// <summary>
        /// PreprocessDocument
        /// </summary>
        public static byte[] PreprocessDocument(String sFileName, byte[] fileContent)
        {
            if (Path.GetExtension(sFileName).ToLower() == ".pdf")
            {

            }

            return fileContent;
        }
    }
}
