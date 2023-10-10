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
        public static TMWriter CreateFileBasedTmWriter(String indexDirectoryPath, bool createNewTmIndex, ILogger logger)
        {
            try
            {
                if (!System.IO.Directory.Exists(indexDirectoryPath))
                {
                    throw new IOException(indexDirectoryPath + " does not exist");
                }

                return new TMWriter(FSDirectory.Open(indexDirectoryPath), createNewTmIndex, logger);
            }
            catch (IOException ex)
            {
                throw new IOException("Trouble creating FSDirectory with the given path: " + indexDirectoryPath, ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static TMWriter CreateRAMBasedTmWriter(ILogger logger)
        {
            try
            {
                //ByteBuffersDirectory directory = new ByteBuffersDirectory();
                RAMDirectory directory = new RAMDirectory();
                TMWriter writer = new TMWriter(directory, true, logger);
                return writer;
            }
            catch (Exception)
            {
                throw new Exception("Failed to create RAM directory.");
            }
        }
    }
}
