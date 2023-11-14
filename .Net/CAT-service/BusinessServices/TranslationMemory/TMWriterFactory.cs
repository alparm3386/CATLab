using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT.TM
{
    public class TmWriterFactory
    {
        public static TMWriter CreateFileBasedTmWriter(String indexDirectoryPath, bool createNewTmIndex)
        {
            try
            {
                if (!System.IO.Directory.Exists(indexDirectoryPath))
                {
                    throw new IOException(indexDirectoryPath + " does not exist");
                }

                return new TMWriter(FSDirectory.Open(indexDirectoryPath), createNewTmIndex);
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static TMWriter CreateRAMBasedTmWriter()
        {
            try
            {
                //ByteBuffersDirectory directory = new ByteBuffersDirectory();
                RAMDirectory directory = new RAMDirectory();
                TMWriter writer = new TMWriter(directory, true);
                return writer;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
