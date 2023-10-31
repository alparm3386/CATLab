package com.tm.okapi.service;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.*;
import java.util.Map.Entry;
import java.util.regex.*;
import org.w3c.dom.*;

import javax.naming.*;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.parsers.*;
import javax.xml.xpath.*;

import org.apache.commons.io.*;
import org.apache.lucene.store.FSDirectory;
import org.w3c.dom.*;
import org.xml.sax.InputSource;

import com.google.gson.Gson;
import com.tm.okapi.filters.cascading.CascadingFilter;
import com.tm.okapi.steps.XliffTranslationStep;
import com.tm.okapi.utils.CATUtils;
import com.tm.okapi.utils.Logging;
import com.tm.okapi.utils.XmlUtils;

import net.sf.okapi.common.*;
import net.sf.okapi.common.annotation.AltTranslation;
import net.sf.okapi.common.annotation.AltTranslationsAnnotation;
import net.sf.okapi.common.filters.*;
import net.sf.okapi.common.filterwriter.*;
import net.sf.okapi.common.pipelinedriver.*;
import net.sf.okapi.common.resource.*;
import net.sf.okapi.filters.tmx.TmxUtils;
import net.sf.okapi.filters.xliff.XLIFFFilter;
import net.sf.okapi.lib.segmentation.SRXDocument;
import net.sf.okapi.steps.common.*;
import net.sf.okapi.steps.scopingreport.ScopingReportStep;
import net.sf.okapi.steps.segmentation.*;
import net.sf.okapi.steps.wordcount.*;
import net.sf.okapi.tm.pensieve.common.*;
import net.sf.okapi.tm.pensieve.seeker.*;
import net.sf.okapi.tm.pensieve.writer.*;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
//[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]

public class OkapiService implements IOkapiService {

	private static int LOG_TIMEOUT = 1000;
	private static String TEMP_DIR = "/data/CAT/Temp";
	private static String SRX_FILE = "/data/CAT/Resources/All languages segmentation.srx";

	public String createXliffFromDocument(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent,
			String sourceLangISO639_1, String targetLangISO639_1) throws Exception {

		String sXliffPath = Paths.get(TEMP_DIR, UUID.randomUUID().toString() + ".xliff").toString();
		try {
			long lStart = System.currentTimeMillis();

			String sExt = FilenameUtils.getExtension(sFileName);
			var filter = createFilterForFile(sExt, sFilterName, filterContent);
			boolean bXliff = filter.getDisplayName().toLowerCase().contains("xliff");
			if (bXliff) {
				try (InputStream is = new ByteArrayInputStream(fileContent)) {
					// fix the source and target languages
					var factory = DocumentBuilderFactory.newInstance();
					factory.setNamespaceAware(false);
					var builder = factory.newDocumentBuilder();
					var xlfDoc = builder.parse(is);
					is.close();
					var fileNodes = xlfDoc.getElementsByTagName("file");

					// reset the source and target languages
					for (int i = 0; i < fileNodes.getLength(); i++) {
						var node = (Element) fileNodes.item(i);
						node.setAttribute("source-language", sourceLangISO639_1);
						node.setAttribute("target-language", targetLangISO639_1);
					}

					var outStream = new ByteArrayOutputStream();
					XmlUtils.writeXml(xlfDoc, outStream);
					fileContent = outStream.toByteArray();
				}
			}

			var fis = new ByteArrayInputStream(fileContent);
			RawDocument rawDoc = new RawDocument(fis, "UTF-8", LocaleId.fromString(sourceLangISO639_1));
			rawDoc.setTargetLocale(LocaleId.fromString(targetLangISO639_1));

			// The segmentation
			var segStep = new SegmentationStep();
			var segParams = (net.sf.okapi.steps.segmentation.Parameters) segStep.getParameters();
			segParams.setSegmentSource(true);
			segParams.setSegmentTarget(true);
			segParams.setSourceSrxPath(SRX_FILE);
			segParams.setTargetSrxPath(SRX_FILE);
			segParams.setCopySource(false);

			PipelineDriver driver = new PipelineDriver();

// Raw document to filter events step
			RawDocumentToFilterEventsStep rd2feStep = new RawDocumentToFilterEventsStep(filter);
			driver.addStep(rd2feStep);

// Add segmentation step if requested
			if (!bXliff)
				driver.addStep(segStep);

// Filter events to raw document final step (using the XLIFF writer)
			FilterEventsWriterStep fewStep = new FilterEventsWriterStep();
			XLIFFWriterParameters paramsXliff;
			XLIFFWriter writer = new XLIFFWriter();
			fewStep.setFilterWriter(writer);
			paramsXliff = writer.getParameters();
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
			System.out.println(DateTimeFormatter.ofPattern("yyyy/MM/dd HH:mm:ss").format(LocalDateTime.now())
					+ " createXliff: " + sFileName + " -> " + lEnd + "ms");
			Logging.Log("filtering.log", DateTimeFormatter.ofPattern("yyyy/MM/dd HH:mm:ss").format(LocalDateTime.now())
					+ " createXliff: " + sFileName + " -> " + lEnd + "ms");
			return sXliffContent;
		} catch (Exception ex) {
			System.out.println("ERROR createXliff: " + sFileName + " -> " + ex.toString() + "ms");
			Logging.Log("Errors.log", DateTimeFormatter.ofPattern("yyyy/MM/dd HH:mm:ss").format(LocalDateTime.now())
					+ " createXliff: " + sFileName + " -> " + ex.toString());
			throw ex;
		} finally {
// cleanup
			var file = new File(sXliffPath);
			if (file.exists())
				try {
					file.delete();
				} catch (Exception ex) {
				}
		}
	}

	public byte[] createDocumentFromXliff(String sFileName, byte[] fileContent, String sFilterName,
			byte[] filterContent, String sourceLangISO639_1, String targetLangISO639_1, String sXliffContent)
			throws Exception {
		String sTempOutPath = null;
		try {
			var lStart = System.currentTimeMillis();

			String sExt = FilenameUtils.getExtension(sFileName);
			var filter = createFilterForFile(sExt, sFilterName, filterContent);
			// fix the languages for xliff
			boolean bXliff = filter.getDisplayName().toLowerCase().contains("xliff");
			if (bXliff) {
				try (InputStream is = new ByteArrayInputStream(fileContent)) {
					// fix the source and target languages
					var factory = DocumentBuilderFactory.newInstance();
					factory.setNamespaceAware(false);
					var builder = factory.newDocumentBuilder();
					var xlfDoc = builder.parse(is);
					is.close();
					var fileNodes = xlfDoc.getElementsByTagName("file");

					// reset the source and target languages
					for (int i = 0; i < fileNodes.getLength(); i++) {
						var node = (Element) fileNodes.item(i);
						node.setAttribute("source-language", sourceLangISO639_1);
						node.setAttribute("target-language", targetLangISO639_1);
					}

					var outStream = new ByteArrayOutputStream();
					XmlUtils.writeXml(xlfDoc, outStream);
					fileContent = outStream.toByteArray();
				}
			}

			// the raw document
			var fis = new ByteArrayInputStream(fileContent);
			RawDocument rawDoc = new RawDocument(fis, "UTF-8", LocaleId.fromString(sourceLangISO639_1));
			rawDoc.setTargetLocale(LocaleId.fromString(targetLangISO639_1));

			// the segmentation
			var segStep = new SegmentationStep();
			var segParams = (net.sf.okapi.steps.segmentation.Parameters) segStep.getParameters();
			segParams.setSegmentSource(true);
			segParams.setSegmentTarget(true);
			segParams.setSourceSrxPath(SRX_FILE);
			segParams.setTargetSrxPath(SRX_FILE);
			// segParams.setCopySource(true);
			segParams.setSegmentTarget(true);

			// the pipeline
			PipelineDriver driver = new PipelineDriver();
			// the raw document to filter events step
			RawDocumentToFilterEventsStep rawDocToFilterEventStep = new RawDocumentToFilterEventsStep(filter);
			driver.addStep(rawDocToFilterEventStep);

			// add segmentation step
			if (!bXliff)
				driver.addStep(segStep);

			// add the translate step
			var translateStep = new XliffTranslationStep(sXliffContent);
			translateStep.setSourceLocale(LocaleId.fromString(sourceLangISO639_1));
			translateStep.setTargetLocale(LocaleId.fromString(targetLangISO639_1));
			driver.addStep(translateStep);

			sTempOutPath = Paths.get(TEMP_DIR, UUID.randomUUID().toString()).toString();

			// Add the filter writer step
			var fewStep = new FilterEventsWriterStep();
			IFilterWriter fw = filter.createFilterWriter();
			fw.setOutput(sTempOutPath);
			fewStep.setFilterWriter(fw);
			driver.addStep(fewStep);

			// Create the raw document and set the output
			driver.addBatchItem(rawDoc, new java.io.File(sTempOutPath).toURI(), null);
			// Process
			driver.processBatch();

			var lElapsed = System.currentTimeMillis() - lStart;
			System.out.println(DateTimeFormatter.ofPattern("yyyy/MM/dd HH:mm:ss").format(LocalDateTime.now())
					+ " createDoc: " + sFileName + " -> " + lElapsed + "ms.");

			var aRet = Files.readAllBytes(Path.of(sTempOutPath));
			return aRet;

		} catch (Exception ex) {
			throw ex;
			// return null;
		} finally {
			// cleanup
			if (sTempOutPath != null)
				try {
					new File(sTempOutPath).delete();
				} catch (Exception ex) {
				}
		}

	}

	private static HashMap<String, String> CreateExtensionMap() {
		var extensionsMap = new HashMap<String, String>();
		// Instead create a map with extensions -> filter
		extensionsMap.put(".docx", "okf_openxml");
		extensionsMap.put(".pptx", "okf_openxml");
		extensionsMap.put(".xlsx", "okf_openxml");

		extensionsMap.put(".odt", "okf_openoffice");
		extensionsMap.put(".swx", "okf_openoffice");
		extensionsMap.put(".ods", "okf_openoffice");
		extensionsMap.put(".swc", "okf_openoffice");
		extensionsMap.put(".odp", "okf_openoffice");
		extensionsMap.put(".sxi", "okf_openoffice");
		extensionsMap.put(".odg", "okf_openoffice");
		extensionsMap.put(".sxd", "okf_openoffice");

		extensionsMap.put(".htm", "okf_html");
		extensionsMap.put(".html", "okf_html");
		extensionsMap.put(".xlf", "okf_xliff");
		extensionsMap.put(".xlif", "okf_xliff");
		extensionsMap.put(".xliff", "okf_xliff");
		extensionsMap.put(".tmx", "okf_tmx");
		extensionsMap.put(".properties", "okf_properties");
		extensionsMap.put(".lang", "okf_properties-skypeLang");
		extensionsMap.put(".po", "okf_po");
		extensionsMap.put(".xml", "okf_xml");
		extensionsMap.put(".resx", "okf_xml-resx");
		extensionsMap.put(".srt", "okf_regex-srt");
		extensionsMap.put(".dtd", "okf_dtd");
		extensionsMap.put(".ent", "okf_dtd");
		extensionsMap.put(".ts", "okf_ts");
		extensionsMap.put(".txt", "okf_plaintext");
		extensionsMap.put(".csv", "okf_table_csv");
		extensionsMap.put(".ttx", "okf_ttx");
		extensionsMap.put(".json", "okf_json");
		extensionsMap.put(".pentm", "okf_pensieve");
		extensionsMap.put(".yml", "okf_yaml");
		extensionsMap.put(".idml", "okf_idml");
		extensionsMap.put(".mif", "okf_mif");
		extensionsMap.put(".txp", "okf_transifex");
		extensionsMap.put(".rtf", "okf_tradosrtf");
		extensionsMap.put(".zip", "okf_archive");
		extensionsMap.put(".txml", "okf_txml");
		extensionsMap.put(".md", "okf_markdown");

		return extensionsMap;
	}

	private static IFilter createFilterForFile(String sFileExt, String sFilterName, byte[] aFilterContent)
			throws Exception {
		IFilter filter = null;
		if (aFilterContent == null || aFilterContent.length == 0) { // create the filter by extension
			switch (sFileExt.toLowerCase()) {
			case "txt":
				filter = new net.sf.okapi.filters.plaintext.PlainTextFilter();
				break;
			case "docx":
			case "pptx":
			case "xlsx":
				filter = new net.sf.okapi.filters.openxml.OpenXMLFilter();
				break;
			case "odt":
			case "swx":
			case "ods":
			case "swc":
			case "odp":
			case "sxi":
			case "odg":
			case "sxd":
			case "ott":
			case "odm":
			case "ots":
			case "otg":
			case "otp":
			case "odf":
			case "odc":
			case "odb":
				filter = new net.sf.okapi.filters.openoffice.OpenOfficeFilter();
				break;
			case "htm":
			case "html":
				filter = new net.sf.okapi.filters.html.HtmlFilter();
				break;
			case "xlf":
			case "xlif":
			case "xliff":
			case "mqxliff":
			case "sdlxliff":
				filter = new net.sf.okapi.filters.xliff.XLIFFFilter();
				break;
			case "xml":
				filter = new net.sf.okapi.filters.xml.XMLFilter();
				break;
			case "pdf":
				filter = new net.sf.okapi.filters.pdf.PdfFilter();
				break;
			case "json":
				filter = new net.sf.okapi.filters.json.JSONFilter();
				break;
			case "idml":
				filter = new net.sf.okapi.filters.idml.IDMLFilter();
				break;
			case "icml":
				filter = new net.sf.okapi.filters.icml.ICMLFilter();
				break;
			case "zip":
				filter = new net.sf.okapi.filters.archive.ArchiveFilter();
				break;
			case "docset":
			case "dox":
			case "doxy":
				filter = new net.sf.okapi.filters.doxygen.DoxygenFilter();
				break;
			case "dtd":
				filter = new net.sf.okapi.filters.dtd.DTDFilter();
				break;
			case "md":
				filter = new net.sf.okapi.filters.markdown.MarkdownFilter();
				break;
			case "mif":
				filter = new net.sf.okapi.filters.mif.MIFFilter();
				break;
			case "csv":
				filter = new net.sf.okapi.filters.multiparsers.MultiParsersFilter();
				break;
			case "php":
				filter = new net.sf.okapi.filters.php.PHPContentFilter();
				break;
			case "po":
				filter = new net.sf.okapi.filters.po.POFilter();
				break;
			case "ts":
				filter = new net.sf.okapi.filters.ts.TsFilter();
				break;
			case "ttx":
				filter = new net.sf.okapi.filters.ttx.TTXFilter();
				break;
			// case "resx":
			// filter = new net.sf.okapi.filters...();
			// break;
			case "yml":
			case "yaml":
				filter = new net.sf.okapi.filters.yaml.YamlFilter();
				break;
			default:
				throw new Exception("File type is not supported.");
			}
		} else {
			var fprmInputStream = new ByteArrayInputStream(aFilterContent);
			filter = com.tm.okapi.filters.DefaultFilters.createFilterByFilterName(sFilterName);
			var parameters = filter.getParameters();
			parameters.load(fprmInputStream, true);
			filter.setParameters(parameters);
		}

		// extensionsMap.Add(".tmx", "okf_tmx");
		// extensionsMap.Add(".properties", "okf_properties");
		// extensionsMap.Add(".lang", "okf_properties-skypeLang");
		// extensionsMap.Add(".po", "okf_po");
		// extensionsMap.Add(".resx", "okf_xml-resx");
		// extensionsMap.Add(".srt", "okf_regex-srt");
		// extensionsMap.Add(".dtd", "okf_dtd");
		// extensionsMap.Add(".ent", "okf_dtd");
		// extensionsMap.Add(".ts", "okf_ts");
		// extensionsMap.Add(".txt", "okf_plaintext");
		// extensionsMap.Add(".csv", "okf_table_csv");
		// extensionsMap.Add(".ttx", "okf_ttx");
		// extensionsMap.Add(".json", "okf_json");
		// extensionsMap.Add(".pentm", "okf_pensieve");
		// extensionsMap.Add(".yml", "okf_yaml");
		// extensionsMap.Add(".idml", "okf_idml");
		// extensionsMap.Add(".mif", "okf_mif");
		// extensionsMap.Add(".txp", "okf_transifex");
		// extensionsMap.Add(".rtf", "okf_tradosrtf");
		// extensionsMap.Add(".zip", "okf_archive");
		// extensionsMap.Add(".txml", "okf_txml");
		// extensionsMap.Add(".md", "okf_markdown");

		return filter;
	}

	private int countWords(TextFragment segment) throws Exception {
		String sText = segment.getText();
		// var wordIterator =
		// (com.ibm.icu.text.RuleBasedBreakIterator)BreakIterator.getWordInstance(ULocale
		// .createCanonical(srcLoc.toString()));
		// com.ibm.icu.text.RuleBasedBreakIterator.registerInstance(srcWordIterator,
		// srcLoc.toJavaLocale(),
		// BreakIterator.KIND_WORD);

		// long totalWordCount = 0;
		// int current = 0;
		// com.ibm.icu.text.RuleBasedBreakIterator wordIterator;

		// if (Util.isEmpty(text))
		// {
		// return totalWordCount;
		// }

		// wordIterator = srcWordIterator;
		// wordIterator.setText(text);

		// while (true)
		// {
		// if (current == com.ibm.icu.text.BreakIterator.DONE)
		// {
		// break;
		// }

		// current = wordIterator.next();
		// // don't count various space and punctuation
		// if (wordIterator.getRuleStatus() !=
		// com.ibm.icu.text.RuleBasedBreakIterator.WORD_NONE)
		// {
		// totalWordCount++;
		// }
		// }

		// fix me! The dodgy word count
		Pattern pattern = Pattern.compile("\\w+", Pattern.CASE_INSENSITIVE);
		Matcher matcher = pattern.matcher(sText);
		int totalWordCount = (int) matcher.results().count();

		return totalWordCount;
	}

}
