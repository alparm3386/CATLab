using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cat.tm
{
    public class TmWriterFactory
    {
        public static TMWriter CreateFileBasedTmWriter(String indexDirectoryPath, bool createNewTmIndex)
        {
            TMWriter writer = null;
            try
            {
                if (!System.IO.Directory.Exists(indexDirectoryPath))
                {
                    throw new IOException(indexDirectoryPath + " does not exist");
                }
                writer = new TMWriter(FSDirectory.Open(indexDirectoryPath), createNewTmIndex);
            }
            catch (IOException ex)
            {
                throw new IOException("Trouble creating FSDirectory with the given path: " + indexDirectoryPath, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return writer;
        }

        public static TMWriter CreateRAMBasedTmWriter()
        {
            TMWriter writer = null;
            try
            {
                //ByteBuffersDirectory directory = new ByteBuffersDirectory();
                RAMDirectory directory = new RAMDirectory();
                writer = new TMWriter(directory, true);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create RAM directory.");
            }
            return writer;
        }
    }
}
