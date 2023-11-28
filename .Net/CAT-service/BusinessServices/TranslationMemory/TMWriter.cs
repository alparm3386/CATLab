using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using cat.utils;
using CAT.Okapi.analysis;
using CAT.Okapi.Resources;
using CAT.Utils;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;
using utils;
using Directory = Lucene.Net.Store.Directory;
using Document = Lucene.Net.Documents.Document;

namespace CAT.TM
{
    /**
	 * Used to write, delete and update the index.
	 */
    public class TMWriter
    {
        private readonly String IdField = "ID";
        private readonly int NGramLength = 4;
        private readonly IndexWriter indexWriter;

        /**
         * Creates a TMWriter
         * 
         * @param indexDirectory   - the Lucene Directory implementation of choice.
         * @throws IOException if the indexDirectory can not load
         */
        public TMWriter(Directory indexDirectory, bool createNewTmIndex)
        {
            var conf = new IndexWriterConfig(LuceneVersion.LUCENE_48, new NgramAnalyzer(NGramLength))
            {
                OpenMode = createNewTmIndex ? OpenMode.CREATE : OpenMode.APPEND,
                Similarity = new DefaultSimilarity() //could be also BooleanSimilarity
            };
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
            finally
            {
                try
                {
                    indexWriter.Dispose();
                }
                catch (Exception)
                {
                    //swallow the exception
                }
            }
        }

        /**
         * {@inheritDoc}
         */
        public void Commit()
        {
            indexWriter.Commit();
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
                var term = new Term(IdField, tuid.ToString());
                indexWriter.DeleteDocuments(term);
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
                throw new ArgumentNullException(nameof(source));

            //delete the entry
            if (id <= 0) // the auto-increment id cannot be zero or negative number
                throw new InvalidOperationException("Invalid id.");

            var doc = new Document();
            //id
            var fieldType = new FieldType
            {
                IndexOptions = IndexOptions.DOCS_ONLY,
                IsStored = true,
                //fieldType.NumericType = NumericType.INT32
                OmitNorms = true,
                IsIndexed = true,
                StoreTermVectors = false,
                IsTokenized = false
            };
            doc.Add(new Field(IdField, id.ToString(), fieldType)); //it tokenizes the Int32Field for some reason

            //source
            fieldType = new FieldType
            {
                IndexOptions = IndexOptions.DOCS_ONLY,
                StoreTermVectors = false,
                IsStored = false,
                OmitNorms = true,
                IsIndexed = true
            };
            var text = source.GetText(); //index the plain text only

            //we index the short texts too
            if (text.Length <= NGramLength)
            {
                text = text.Replace("  ", " ");
                text = text.PadRight(NGramLength, '$');
            }
            doc.Add(new Field(TranslationUnitField.SOURCE.ToString(), text, fieldType));
            var stringTerms = CATUtils.GetTermsFromText(text);

            //TermsNum
            var uniqeTerms = new HashSet<String>(stringTerms);
            doc.Add(new NumericDocValuesField("TermsNum", uniqeTerms.Count));

            //specialities
            doc.Add(new BinaryDocValuesField("Specialities", new BytesRef(specialities)));
            return doc;
        }
    }
}
