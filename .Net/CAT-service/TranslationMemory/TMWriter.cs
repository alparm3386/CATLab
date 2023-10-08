using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using cat.utils;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;
using okapi.resource;
using okapi.search.analysis;
using utils;
using Directory = Lucene.Net.Store.Directory;
using Document = Lucene.Net.Documents.Document;

namespace cat.tm
{
    /**
	 * Used to write, delete and update the index.
	 */
    public class TMWriter
    {
        private readonly String ID_FIELD = "ID";
        private readonly String SOURCE_FIELD = "SOURCE";
        private readonly int N_GRAM_LEN = 4;
        private IndexWriter indexWriter;
        private int maxVersions = 5;
        private Logger logger = new Logger();

        /**
         * Creates a TMWriter
         * 
         * @param indexDirectory   - the Lucene Directory implementation of choice.
         * @throws IOException if the indexDirectory can not load
         */
        public TMWriter(Directory indexDirectory, bool createNewTmIndex)
        {
            IndexWriterConfig conf = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, new NgramAnalyzer(N_GRAM_LEN));
            conf.OpenMode = createNewTmIndex ? OpenMode.CREATE : OpenMode.APPEND;
            conf.Similarity = new DefaultSimilarity(); //new BooleanSimilarity();
            indexWriter = new IndexWriter(indexDirectory, conf);
        }

        /**
         * Commits and closes (for now) the transaction.
         * 
         * @throws IOException if the commit cannot happen.
         */
        public void Close()
        {
            try
            {
                indexWriter.Commit();
            }
            catch (Exception ex)
            {
                throw ex; // To change body of catch statement use File | Settings | File Templates.
            }
            finally
            {
                try
                {
                    indexWriter.Dispose();
                }
                catch (IOException ignored)
                {
                    logger.Warn("Exception closing Pensieve IndexWriter." + ignored.ToString()); //$NON-NLS-1$
                }
            }
        }

        /**
         * {@inheritDoc}
         */
        public void Commit()
        {
            try
            {
                indexWriter.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /**
         * Gets a handle on the IndexWriter so that commits and rollbacks can happen
         * outside. For now, this is a convenience method. In other words, don't depend
         * on it working for you.
         * 
         * @return a handle on the IndexWriter used to Create, Update or Delete the
         *         index.
         */
        public IndexWriter IndexWriter
        {
            get { return indexWriter; }
        }

        public void IndexSource(int id, TextFragment source, String specialities)
        {
            Document doc = CreateDocument(id, source, specialities);
            if (doc != null)
            {
                try
                {
                    indexWriter.AddDocument(doc);
                }
                catch (CorruptIndexException ex)
                {
                    throw new IOException("Error adding to the TM. Corrupted index.", ex);
                }
                catch (IOException ex)
                {
                    throw new IOException("Error adding to the TM.", ex);
                }
            }
        }

        public void Delete(int tuid)
        {
            try
            {
                //BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                //NumericUtils.Int32ToPrefixCoded(tuid, 0, bytes);
                //indexWriter.DeleteDocuments(new Term(ID_FIELD, bytes));

                var term = new Term(ID_FIELD, tuid.ToString());
                indexWriter.DeleteDocuments(term);
                //indexWriter.Commit();
            }
            catch (CorruptIndexException e)
            {
                throw new IOException("Error deleting item from the TM. Corrupted index.", e);
            }
            catch (IOException e)
            {
                throw new IOException("Error deleting item from the TM.", e);
            }
        }

        /**
         * Creates a document for a given translation unit, including inline codes.
         * 
         * @param tu the translation unit used to create the document.
         * @return a new document.
         */
        Document CreateDocument(int id, TextFragment source, String specialities)
        {
            if (source == null)
                throw new NullReferenceException("The text cannot be null");

            //delete the entry
            if (id <= 0) // the auto-increment id cannot be zero or negative number
                throw new NullReferenceException("Invalid id.");

            Document doc = new Document();
            //id
            FieldType fieldType = new FieldType();
            fieldType.IndexOptions = IndexOptions.DOCS_ONLY;
            fieldType.IsStored = true;
            //fieldType.NumericType = NumericType.INT32;
            fieldType.OmitNorms = true;
            fieldType.IsIndexed = true;
            fieldType.StoreTermVectors = false;
            fieldType.IsTokenized = false;
            doc.Add(new Field(ID_FIELD, id.ToString(), fieldType)); //it tokenizes the Int32Field for some reason

            //source
            fieldType = new FieldType();
            fieldType.IndexOptions = IndexOptions.DOCS_ONLY;
            fieldType.StoreTermVectors = false;
            fieldType.IsStored = false;
            fieldType.OmitNorms = true;
            fieldType.IsIndexed = true;
            var text = source.GetText(); //index the plain text only

            //we index the short texts too
            if (text.Length <= N_GRAM_LEN)
            {
                text = text.Replace("  ", " ");
                text = text.PadRight(N_GRAM_LEN, '$');
            }
            doc.Add(new Field(TranslationUnitField.SOURCE.ToString(), text, fieldType));
            var stringTerms = CATUtils.GetTermsFromText(text);

            //TermsNum
            //var terms = stringTerms.ConvertAll(term => new Term(TranslationUnitField.SOURCE.ToString(), term));
            var uniqeTerms = new HashSet<String>(stringTerms);
            doc.Add(new NumericDocValuesField("TermsNum", uniqeTerms.Count));
            //if (uniqeTerms.Count == 0)
            //{
            //    System.Console.WriteLine("Should never happen.");
            //}

            //specialities
            doc.Add(new BinaryDocValuesField("Specialities", new BytesRef(specialities)));
            return doc;
        }
    }
}
