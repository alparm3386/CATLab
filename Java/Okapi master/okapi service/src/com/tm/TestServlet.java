package com.tm;

import java.io.BufferedReader;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.StringReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.List;
import java.util.Locale;
import java.util.UUID;
import java.util.stream.Collectors;

import javax.servlet.annotation.WebServlet;
import javax.servlet.http.*;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;

import org.apache.commons.io.FilenameUtils;
import org.apache.lucene.analysis.TokenStream;
import org.apache.lucene.analysis.tokenattributes.TermAttribute;
import org.apache.lucene.index.IndexReader;
import org.apache.lucene.index.Term;
import org.apache.lucene.index.TermPositions;
import org.apache.lucene.search.IndexSearcher;
import org.apache.lucene.store.FSDirectory;
import org.apache.lucene.store.RAMDirectory;
import org.apache.lucene.util.OpenBitSet;
import org.apache.lucene.util.OpenBitSetIterator;
import org.w3c.dom.*;

import com.tm.okapi.service.*;
import com.tm.okapi.tm.PensieveTM;
import com.tm.okapi.utils.CATUtils;
import com.tm.okapi.utils.XmlUtils;

import gnu.trove.map.hash.TIntIntHashMap;
import net.sf.okapi.common.Event;
import net.sf.okapi.common.EventType;
import net.sf.okapi.common.LocaleId;
import net.sf.okapi.common.filters.IFilter;
import net.sf.okapi.common.filterwriter.XLIFFWriter;
import net.sf.okapi.common.filterwriter.XLIFFWriterParameters;
import net.sf.okapi.common.pipelinedriver.PipelineDriver;
import net.sf.okapi.common.resource.RawDocument;
import net.sf.okapi.common.resource.TextFragment;
import net.sf.okapi.common.resource.TextUnit;
import net.sf.okapi.filters.html.HtmlFilter;
import net.sf.okapi.lib.search.lucene.analysis.NgramAnalyzer;
import net.sf.okapi.lib.search.lucene.query.TmFuzzyQuery;
import net.sf.okapi.lib.search.lucene.scorer.TmFuzzyScorer;
import net.sf.okapi.steps.common.FilterEventsWriterStep;
import net.sf.okapi.steps.common.RawDocumentToFilterEventsStep;
import net.sf.okapi.steps.segmentation.SegmentationStep;
import net.sf.okapi.tm.pensieve.common.TranslationUnit;
import net.sf.okapi.tm.pensieve.common.TranslationUnitField;
import net.sf.okapi.tm.pensieve.common.TranslationUnitVariant;
import net.sf.okapi.tm.pensieve.seeker.*;
import net.sf.okapi.tm.pensieve.writer.*;

@WebServlet("/test")
public class TestServlet extends HttpServlet {
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	/*
	 * private static void testHomogeneity() throws Exception { var sFilePath =
	 * "C:\\Development\\OKAPI\\Test files\\Smythson xmls mq.xliff.xlf"; var
	 * lstTextFragments = readXliff(sFilePath);
	 * 
	 * var start = System.currentTimeMillis(); // lucene var tmWriter =
	 * (PensieveWriter) TmWriterFactory.createRAMBasedTmWriter(); PensieveSeeker
	 * homogeneityTm = new PensieveSeeker(tmWriter.getIndexWriter());
	 * 
	 * for (var textFragment : lstTextFragments) { var ntu = new TranslationUnit(new
	 * TranslationUnitVariant(new LocaleId("en"), textFragment), null);
	 * tmWriter.indexTranslationUnit(ntu, true); }
	 * 
	 * tmWriter.commit(); System.out.println("lucene homogeneity index: " +
	 * (System.currentTimeMillis() - start) + "ms.");
	 * 
	 * //simple indexer start = System.currentTimeMillis(); var homogeneityIndex =
	 * new InvertedIndex(); for (TextFragment textFragment : lstTextFragments) {
	 * List<String> terms = homogeneityIndex.GetUniqueTerms(textFragment);
	 * homogeneityIndex.AddToIndex(textFragment); }
	 * System.out.println("homogeneity: " + (System.currentTimeMillis() - start) +
	 * "ms.");
	 * 
	 * }
	 */
	private static void testCreateXliff() {
		try {
			OkapiService okapiService = new OkapiService();
			// var sPath = "C:\\Alpar\\Smythson xmls mq.xliff";
			// var sPath = "C:\\Alpar\\SDL\\test.xliff";
			var sPath = "C:\\Alpar\\Aspose + MS Word.docx";
			var aFileContent = Files.readAllBytes(Path.of(sPath));
			String sFilterName = null;
			byte[] filterContent = null;
			// byte[] filterContent = Files
			// .readAllBytes(Path.of("C:\\Development\\Okapi\\Test
			// files\\okf_openxml@test.fprm"));
			var sSource = "en";
			var sTarget = "fr";
			// var aTMAssignments = new TMAssignment[1];
			// var tm1 = new TMAssignment();
			// tm1.TMName = "__Okapi test account_eng_fre_Marketing.pentm";
			// aTMAssignments[0] = tm1;
			var outXliff = okapiService.createXliff(FilenameUtils.getName(sPath), aFileContent, sFilterName,
					filterContent, sSource, sTarget, null);

			int a = 0;
		} catch (Exception ex) {
			int a = 0;
		}
	}

	public static void testCreateDoc() {
		try {
			var sPath = "C:\\Alpar\\3258044_translatedDocument.xlf";
			var sXliffContent = Files.readString(Path.of(sPath));
			OkapiService okapiService = new OkapiService();
			String filePath = "C:/Alpar/document.xliff";
			File file = new File(filePath);
			var aFileContent = Files.readAllBytes(Path.of(filePath));
			okapiService.createDocumentFromXliff("test.xliff", aFileContent, "", null, "en", "id", sXliffContent);
			int a = 0;
		} catch (Exception ex) {
			int a = 0;
		}
	}

	private static void testStats() {
		try {
			CorpusEntry ce = new CorpusEntry();
			OkapiService okapiService = new OkapiService();
			// var sPath = "C:\\Development\\Okapi\\Test files\\Job535027.xlsx";
			var sPath = "C:\\Development\\Okapi\\Test files\\Smythson xmls mq.xliff"; //
			var aFileContent = Files.readAllBytes(Path.of(sPath));
			var sSource = "en";
			var sTarget = "fr";

			var aTMAssignments = new ArrayList<TMAssignment>();
			var tm1 = new TMAssignment();
			// tm1.TMName = "__Smythson E-mail and marketing translations_eng_fre.pentm";
			tm1.TMName = "__Smythson_eng_fre.pentm";
			aTMAssignments.add(tm1);

			// TMExists
			// var bExists = okapiService.TMExists("__Okapi test
			// account_eng_fre_Marketing.pentm");
			// CreateXliff
			// String sOut = okapiService.CreateXliff("test.xlsx", aFileContent, "", null,
			// sSource, sTarget, null);
			// GetStatisticsForDocument
			var stats = okapiService.getStatisticsForDocument(FilenameUtils.getName(sPath), aFileContent, "", null,
					sSource, new String[] { sTarget }, aTMAssignments.toArray(new TMAssignment[aTMAssignments.size()]));

			int a = 0;
		} catch (Exception ex) {
			int a = 0;
		}
	}

	public static void importTM() {
		try {
			var penTM = new PensieveTM();
			String sTMName = "__3.3.0-Smythson_eng_fre.pentm";
			if (!penTM.TMExists(sTMName))
				penTM.CreateTM(sTMName, "en", "fr");
			var sTMxContent = Files.readString(Path.of("C:/Development/OKAPI/TMs/__Smythson_eng_fre_Marketing.tmx"),
					StandardCharsets.UTF_8);
			penTM.importTmx(sTMName, sTMxContent);
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	/*
	 * public static void getSpecialStat(List<TextFragment> textFragments, boolean
	 * bWithHomogeneity, int threshold) { try { long lStart =
	 * System.currentTimeMillis(); var loopCntr1 = 0; var loopCntr2 = 0; String
	 * searchFieldName = TranslationUnitField.SOURCE.toString(); // inverted index
	 * on source ArrayList<List<String>> textFragmentsUniqueTermsList = new
	 * ArrayList<List<String>>(); var homogeneityIndex = new InvertedIndex(); for
	 * (TextFragment textFragment : textFragments) { List<String> terms =
	 * homogeneityIndex.GetUniqueTerms(textFragment);
	 * textFragmentsUniqueTermsList.add(terms); //float score =
	 * homogeneityIndex.SearchFuzzy(terms, 50);
	 * homogeneityIndex.AddToIndex(textFragment); }
	 * System.out.println("homogeneity: " + (System.currentTimeMillis() - lStart) +
	 * "ms.");
	 * 
	 * // get the documents that contains the terms var termsDocuments = new
	 * HashMap<String, List<Integer>>(); var uniqueTerms =
	 * homogeneityIndex.GetUniqueTermsFromIndex(); // var sTMPath =
	 * "C:\\Development\\OKAPI\\TMs\\__3.0.3-Smythson_eng_fre.pentm"; // var sTMPath
	 * = "C:\\Development\\OKAPI\\TMs\\__3.0.3-Superdry - eCommerce //
	 * Pages_eng_fre_Marketing.pentm"; var sTMPath =
	 * "C:\\Development\\OKAPI\\TMs\\__3.0.3-Starwood Digital US_eng-US_spa.pentm";
	 * var file = new File(sTMPath); var dir = FSDirectory.open(file); var reader =
	 * IndexReader.open(dir, true);
	 * 
	 * // document the lookup table IndexReader[] subReaders =
	 * reader.getSequentialSubReaders(); for (var term : uniqueTerms) { var
	 * documents = new ArrayList<Integer>(); termsDocuments.put(term, documents);
	 * for (int i = 0; i < subReaders.length; i++) { var subReader = subReaders[i];
	 * int freq = reader.docFreq(new Term(searchFieldName, term)); //if (freq > -1)
	 * // continue; TermPositions termPositions = subReader.termPositions(new
	 * Term(searchFieldName, term)); // get the doc candidates //while
	 * (termPositions.next()) { // documents.add(termPositions.doc()); //
	 * loopCntr1++; //} //if (freq != documents.size()) {
	 * //System.out.println("Ooops ..."); } } }
	 * 
	 * var roughThresholdFreq = 0; for (var terms : textFragmentsUniqueTermsList) {
	 * // initialize buffers var scoredDocs = new TIntIntHashMap(); OpenBitSet
	 * docPointers = new OpenBitSet(reader.maxDoc()); roughThresholdFreq = (int)
	 * (terms.size() >> 1); for (var term : terms) { var docIds =
	 * termsDocuments.get(term); if(docIds.size() > 500) continue; for (var docId :
	 * docIds) { loopCntr2++; // var matchedTerms =
	 * scoredDocs.adjustOrPutValue(docId, 1, 1); // if (matchedTerms >
	 * roughThresholdFreq) { // docPointers.fastSet(docId); } } } }
	 * 
	 * // export term / doc nums var bDataDump = false; if (bDataDump) { var
	 * sbTermsDocs = new StringBuilder(); for (int i = 0; i < uniqueTerms.size();
	 * i++) { sbTermsDocs.append(uniqueTerms.get(i) + "\t" +
	 * termsDocuments.get(i).size() + "\n"); }
	 * Files.writeString(Path.of("C:/Alpar/terms.dat"), sbTermsDocs.toString()); }
	 * System.out.println("loopCntr1: " + loopCntr1 + " loopCntr2: " + loopCntr2);
	 * int a = 0; } catch (Exception ex) { System.out.println(ex.toString()); } }
	 */
	public static ArrayList<TextFragment> readXliff(String sXliffPath) throws Exception {
		var lStart = System.currentTimeMillis();
		var sFilePath = "C:\\Development\\OKAPI\\Test files\\Smythson xmls mq.xliff.xlf";
		var xmlFile = new File(sXliffPath);
		var factory = DocumentBuilderFactory.newInstance();
		var builder = factory.newDocumentBuilder();
		var tmpXliff = builder.parse(xmlFile);

		var lstTextFragments = new ArrayList<TextFragment>();
		var tus = tmpXliff.getElementsByTagName("trans-unit");
		for (int i = 0; i < tus.getLength(); i++) {
			var tu = (Element) tus.item(i); // check the segmentation
			var ssNode = (Element) tu.getElementsByTagName("seg-source").item(0);
			var segmentNodes = ssNode.getElementsByTagName("mrk");
			for (int j = 0; j < segmentNodes.getLength(); j++) {
				var segmentNode = (Element) segmentNodes.item(j); // the source segmet
				var source = CATUtils.XliffSegmentToTextFragmentSimple(XmlUtils.getInnerXMLString(segmentNode));
				lstTextFragments.add(source);
			}
		}
		System.out.println("xml read: " + (System.currentTimeMillis() - lStart) + "ms.");

		return lstTextFragments;
	}

	/*
	 * public static void testStatistics() throws Exception { var sFilePath =
	 * "C:\\Development\\OKAPI\\Test files\\Smythson xmls mq.xliff.xlf"; var
	 * lstTextFragments = readXliff(sFilePath);
	 * 
	 * // var sTMPath =
	 * "C:\\Development\\OKAPI\\TMs\\__3.0.3-Smythson_eng_fre.pentm"; // var sTMPath
	 * = "C:\\Development\\OKAPI\\TMs\\__3.0.3-Superdry - eCommerce //
	 * Pages_eng_fre_Marketing.pentm"; var sTMPath =
	 * "C:\\Development\\OKAPI\\TMs\\__3.0.3-Starwood Digital US_eng-US_spa.pentm";
	 * var file = new File(sTMPath); // var file = new
	 * File("C:\\Development\\OKAPI\\TMs\\__No data_eng_fre.pentm"); var dir =
	 * FSDirectory.open(file); var RAMDir = new RAMDirectory(dir); var homogeneityTm
	 * = new PensieveSeeker(dir); homogeneityTm.setNrtMode(false); var start =
	 * System.currentTimeMillis(); NgramAnalyzer defaultFuzzyAnalyzer = new
	 * NgramAnalyzer(new Locale("en"), 4); for (int i = 0; i <
	 * lstTextFragments.size(); i++) { var source = lstTextFragments.get(i); var
	 * tmHits = homogeneityTm.searchFuzzy(source, 50, 1, null); if (true) continue;
	 * }
	 * 
	 * var elapsed = System.currentTimeMillis() - start;
	 * System.out.println("token analizer: " + elapsed + "ms.");
	 * System.out.println("loopCntr: " + TmFuzzyScorer.loopCntr + " queryCntr: " +
	 * TmFuzzyScorer.queryCntr + " scoreCntr: " + TmFuzzyScorer.scoreCntr);
	 * TmFuzzyScorer.loopCntr = 0; TmFuzzyScorer.queryCntr = 0;
	 * TmFuzzyScorer.scoreCntr = 0; homogeneityTm.close(); }
	 */

	/*
	 * public static void testStatisticsNew() throws Exception { var lStart =
	 * System.currentTimeMillis(); var sFilePath =
	 * "C:\\Development\\OKAPI\\Test files\\Smythson xmls mq.xliff.xlf"; var
	 * lstTextFragments = readXliff(sFilePath);
	 * 
	 * // var sTMPath =
	 * "C:\\Development\\OKAPI\\TMs\\__3.0.3-Smythson_eng_fre.pentm"; // var sTMPath
	 * = "C:\\Development\\OKAPI\\TMs\\__3.0.3-Superdry - eCommerce //
	 * Pages_eng_fre_Marketing.pentm"; var sTMPath =
	 * "C:\\Development\\OKAPI\\TMs\\__3.0.3-Starwood Digital US_eng-US_spa.pentm";
	 * var file = new File(sTMPath); var dir = FSDirectory.open(file); // var RAMDir
	 * = new RAMDirectory(dir);
	 * 
	 * var seeker = new PensieveSeeker(dir);
	 * seeker.GetStatisticsForTextFragments(lstTextFragments, true, 50);
	 * System.out.println("total: " + (System.currentTimeMillis() - lStart) +
	 * "ms.");
	 * 
	 * // System.in.read(); }
	 */
	/*
	 * public static void testStatisticsNewest() throws Exception { var lStart =
	 * System.currentTimeMillis(); var sFilePath =
	 * "C:\\Development\\OKAPI\\Test files\\Smythson xmls mq.xliff.xlf"; var
	 * lstTextFragments = readXliff(sFilePath); getSpecialStat(lstTextFragments,
	 * true, 50); System.out.println("total: " + (System.currentTimeMillis() -
	 * lStart) + "ms."); }
	 */

	/*
	 * public static void perfTests() { var perfTests = new PerfTests();
	 * perfTests.testMaps(); }
	 */

	public void simpleFilter() {
		try {
			// Create a filter object
			IFilter filter = new HtmlFilter();
			// Creates the RawDocument object
			String filePath = "C:/Development/Okapi/Test Files/Email.html";
			File file = new File(filePath);
			var aFileContent = Files.readAllBytes(Path.of(filePath));
			var fis = new ByteArrayInputStream(aFileContent);
			RawDocument rawDoc = new RawDocument(fis, "UTF-8", LocaleId.fromString("en"));
			// Opens the document
			filter.open(rawDoc);
			// Get the events from the input document
			while (filter.hasNext()) {
				Event event = filter.next();
				// do something with the event...
				// Here, if the event is TEXT_UNIT, we display the key and the extracted text
				if (event.getEventType() == EventType.TEXT_UNIT) {
					TextUnit tu = (TextUnit) event.getResource();
					System.out.println("--");
					System.out.println("key=[" + tu.getName() + "]");
					System.out.println("text=[" + tu.getSource() + "]");
				}
			}
			// Close the input document
			filter.close();
		} catch (Exception ex) {

		}
	}

	public String pipelineTest() throws Exception {

		try {
			long lStart = System.currentTimeMillis();

			IFilter filter = new HtmlFilter();
			// Creates the RawDocument object
			var filePath = "C:/Development/Okapi/Test Files/Email.html";
			var sXliffPath = "C:/Development/Okapi/Test Files/Email.xliff";

			var aFileContent = Files.readAllBytes(Path.of(filePath));
			var fis = new ByteArrayInputStream(aFileContent);
			RawDocument rawDoc = new RawDocument(fis, "UTF-8", LocaleId.fromString("en"));

			// File file = new File(filePath);
			// RawDocument rawDoc = new RawDocument(file.toURI(), "UTF-8",
			// LocaleId.fromString("en"));
			rawDoc.setTargetLocale(LocaleId.fromString("fr"));

			// filter.open(rawDoc);

			// The segmentation
			var segStep = new SegmentationStep();
			var segParams = (net.sf.okapi.steps.segmentation.Parameters) segStep.getParameters();
			segParams.setSegmentSource(true);
			segParams.setSegmentTarget(true);
			segParams.setSourceSrxPath("C:/Development/Okapi/All languages segmentation.srx");
			segParams.setTargetSrxPath("C:/Development/Okapi/All languages segmentation.srx");
			segParams.setCopySource(false);

			PipelineDriver driver = new PipelineDriver();

			// Raw document to filter events step
			RawDocumentToFilterEventsStep rd2feStep = new RawDocumentToFilterEventsStep(filter);
			driver.addStep(rd2feStep);

			// Add segmentation step if requested
			// driver.addStep(segStep);

			// Filter events to raw document final step (using the XLIFF writer)
			FilterEventsWriterStep fewStep = new FilterEventsWriterStep();
			XLIFFWriterParameters paramsXliff;
			XLIFFWriter writer = new XLIFFWriter();
			fewStep.setFilterWriter(writer);
			paramsXliff = writer.getParameters();
			paramsXliff.setPlaceholderMode(false);
			paramsXliff.setIncludeAltTrans(true);
			paramsXliff.setIncludeCodeAttrs(true);
			paramsXliff.setCopySource(false);
			paramsXliff.setCreateEmptyTarget(true);
			paramsXliff.setPlaceholderMode(false); // don't use 'g' mode.

			// fewStep.setDocumentRoots(rootDir);
			driver.addStep(fewStep);

			// Create the raw document and set the output
			driver.addBatchItem(rawDoc, new java.io.File(sXliffPath).toURI(), "UTF-8");
			// Process
			driver.processBatch();

			String sXliffContent = Files.readString(Path.of(sXliffPath));

			long lEnd = System.currentTimeMillis() - lStart;
			System.out.println("createXliff: " + lEnd + "ms");
			return sXliffContent;
		} catch (Exception ex) {
			throw ex;
		} finally {
			// cleanup
		}
	}

	public void service(HttpServletRequest req, HttpServletResponse res) {
		try {
			// pipelineTest();
			// simpleFilter();
			testCreateDoc();
			// testCreateXliff();
			// testStats();
			// importTM();
			// testHomogeneity();
			// testStatistics();
			// testStatisticsNew();
			// testStatisticsNewest();
			// perfTests();
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
}
