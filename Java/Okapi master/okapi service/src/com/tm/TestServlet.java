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
