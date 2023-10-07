package com.tm.okapi.tm;

import java.io.*;
import java.lang.reflect.Type;
import java.net.URI;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.*;
import java.util.regex.*;
import java.util.stream.Stream;

import javax.naming.*;
import javax.xml.parsers.*;
import javax.xml.soap.SOAPFault;
import javax.xml.ws.soap.SOAPFaultException;
import javax.xml.xpath.*;

import org.apache.axis.utils.XMLUtils;
import org.apache.commons.io.FilenameUtils;
import org.apache.lucene.index.Term;
import org.apache.lucene.search.IndexSearcher;
import org.apache.lucene.search.TermQuery;
import org.w3c.dom.*;
import org.xml.sax.InputSource;

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import com.tm.okapi.service.*;
import com.tm.okapi.utils.*;

import net.sf.okapi.common.*;
import net.sf.okapi.common.filters.*;
import net.sf.okapi.common.filterwriter.TMXWriter;
import net.sf.okapi.common.pipelinedriver.*;
import net.sf.okapi.common.query.*;
import net.sf.okapi.common.resource.*;
import net.sf.okapi.connectors.pensieve.*;
import net.sf.okapi.filters.tmx.TmxUtils;
import net.sf.okapi.steps.common.*;
import net.sf.okapi.steps.formatconversion.*;
import net.sf.okapi.steps.formatconversion.Parameters;
import net.sf.okapi.tm.pensieve.common.*;
import net.sf.okapi.tm.pensieve.seeker.PensieveSeeker;
import net.sf.okapi.tm.pensieve.tmx.OkapiTmxExporter;
import net.sf.okapi.tm.pensieve.writer.*;

public class PensieveTM {
	private static Object TM_LOCK = new Object();

	// fix me! FaultException
	class FaultException extends Exception {
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public FaultException(String message) {
			super(message);
		}
	}

	private static class MetadataMapper {
		private static Gson GSON = new Gson();
		private static Type MTYPE = new TypeToken<HashMap<String, String>>() {
		}.getType();

		public static void fillMetadataFromJson(TranslationUnit tu, String jsonMetadata) {
			throw new UnsupportedOperationException();
		}

		public static HashMap<String, String> metadataToHashMap(String jsonMetadata) {
			var result = new HashMap<String, String>();
			HashMap<String, String> metadata = GSON.fromJson(jsonMetadata, MTYPE);
			for (var key : metadata.keySet()) {
				result.put(key, metadata.get(key));
			}

			return result;
		}
	}

	private class TMConnection {
		public String tmName;
		public PensieveWriter tmWriter;
		public PensieveTMConnector tmQuery;
		public Date created;
		public Date lastAccess;
	}

	private static int LOG_TIMEOUT = 1000;
	private static String REPOSITORY_FOLDER = "";
	private static String TEMP_DIR = "";
	private static HashMap<String, TMConnection> TM_CONNECTION_POOL = new HashMap<String, TMConnection>();

	static {
		Context env;
		try {
			env = (Context) new InitialContext().lookup("java:comp/env");
			// Get a single value
			REPOSITORY_FOLDER = (String) env.lookup("TMPath");
			TEMP_DIR = (String) env.lookup("TempDir");
		} catch (NamingException ex) {
			// TODO Auto-generated catch block
			ex.printStackTrace();
		}
	}

	/// <summary>
	/// FixTMName
	/// </summary>
	/// <param name="sTMName"></param>FixTMName
	/// <returns></returns>
	private String FixTMName(String sTMName) {
		if (!FilenameUtils.getExtension(sTMName).toLowerCase().equals("pentm"))
			sTMName += ".pentm";

		return sTMName;
	}

	/// <summary>
	/// TMExists
	/// </summary>
	/// <param name="sTMName"></param>
	/// <returns></returns>
	public boolean TMExists(String sTMName) {
		sTMName = FixTMName(sTMName);
		return Files.exists(Paths.get(REPOSITORY_FOLDER, sTMName));
	}

	/// <summary>
	/// CreateTM
	/// </summary>
	/// <param name="sTMName"></param>
	/// <param name="sSourceLangIso639_1"></param>
	/// <param name="sTargetLangIso639_1"></param>
	/// <returns></returns>
	public boolean CreateTM(String sTMName, String sSourceLangIso639_1, String sTargetLangIso639_1) {
		ITmWriter tmWriter = null;
		PrintWriter infoFile = null;
		try {
			sTMName = FixTMName(sTMName);
			String tmPath = Paths.get(REPOSITORY_FOLDER, sTMName).toString();
			File tmDir = new File(tmPath);
			if (!tmDir.exists())
				tmDir.mkdirs();
			// create the TM
			tmWriter = TmWriterFactory.createFileBasedTmWriter(tmPath, true);

			// basic meta data
			infoFile = new PrintWriter(Paths.get(REPOSITORY_FOLDER, sTMName).toString() + "\\tmInfo.xml");
			infoFile.println("<tmInfo>\n\t<sourceLocale>" + sSourceLangIso639_1 + "</sourceLocale>\n\t<targetLocale>"
					+ sTargetLangIso639_1 + "</targetLocale></tmInfo>");
			infoFile.close();
		} catch (Exception ex) {
			System.out.println(ex.toString());
			Logging.Log("TMErrors.log", "ERROR: CreateTM -> TM name: " + sTMName + "\n" + ex.toString());
			return false;
		} finally {
			if (tmWriter != null) {
				try {
					tmWriter.close();
				} catch (Exception ex) {
				}
			}
		}

		return true;
	}

	private PensieveWriter GetTMWriter(String sTMName) {
		synchronized (TM_LOCK) {
			try {
				PensieveWriter tmWriter = null;
				sTMName = FixTMName(sTMName).toLowerCase();
				// check if there is open connection
				if (TM_CONNECTION_POOL.containsKey(sTMName)) {
					var tmConnection = TM_CONNECTION_POOL.get(sTMName);
					tmWriter = (PensieveWriter) tmConnection.tmWriter;
					tmConnection.lastAccess = new Date();
				} else {
					// create new connection
					String penTMDirectory = Paths.get(REPOSITORY_FOLDER, sTMName).toString();
					tmWriter = (PensieveWriter) TmWriterFactory.createFileBasedTmWriter(penTMDirectory, false);
					TMConnection tmConnection = new TMConnection();
					tmConnection.tmName = sTMName;
					tmConnection.tmWriter = tmWriter;
					tmConnection.created = new Date();
					tmConnection.lastAccess = new Date();

					TM_CONNECTION_POOL.put(sTMName, tmConnection);
				}

				return tmWriter;
			} catch (Exception ex) {
				throw ex;
			}
		}
	}

	private void updateTMReader(String sTMName) {
		throw new UnsupportedOperationException();
	}

	public IQuery[] GetTMConnectors(String[] aTMNames) throws Exception {
		throw new UnsupportedOperationException();
	}

	public TMHit[] QueryTM(String[] aTMNames, TextFragment source, TextFragment prev, TextFragment next,
			byte matchThreshold, int maxHits) throws Exception {
		try {
			var tmConnectors = GetTMConnectors(aTMNames);
			var tmHits = QueryTM(tmConnectors, source, prev, next, matchThreshold, maxHits);

			// trim the list
			// tmHits = Arrays.copyOfRange(tmHits, 0, Math.min(tmHits.length, maxHits));

			return tmHits;
		} catch (Exception ex) {
			throw ex;
		}
	}

	public TMHit[] QueryTM(IQuery[] tmConnectors, TextFragment source, TextFragment prev, TextFragment next,
			byte matchThreshold, int maxHits) {
		throw new UnsupportedOperationException();
	}

	public TMInfo[] GetTMList(String sTMNameContains) throws FaultException {
		try {
			return null;
		} catch (Exception ex) {
			throw new FaultException(ex.getMessage());
		}
	}

	public TMImportResult importTmx(String sTMName, String sTmxContent) throws FaultException, IOException {
		long lStart = System.currentTimeMillis();
		// get the number of entries before the import
		int itemsBefore = GetTMWriter(sTMName).getIndexWriter().numDocs();
		try {
			// get the languages
			var tmInfo = GetTMInfo(sTMName);
			var sourceLang = tmInfo.langFrom;
			var targetLang = tmInfo.langTo;

			// read the entries from the tmx
			var gson = new Gson();
			var tmEntries = new ArrayList<TMEntry>();
			DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
			factory.setNamespaceAware(false);
			factory.setFeature("http://xml.org/sax/features/namespaces", false);
			factory.setFeature("http://xml.org/sax/features/validation", false);
			factory.setFeature("http://apache.org/xml/features/nonvalidating/load-dtd-grammar", false);
			factory.setFeature("http://apache.org/xml/features/nonvalidating/load-external-dtd", false);
			DocumentBuilder builder = factory.newDocumentBuilder();
			var tmx = builder.parse(new ByteArrayInputStream(sTmxContent.getBytes(StandardCharsets.UTF_8)));
			var tus = tmx.getElementsByTagName("tu");
			for (int i = 0; i < tus.getLength(); i++) {
				var tu = (Element) tus.item(i);
				var tuvs = tu.getElementsByTagName("tuv");
				// create a new entry
				var tmEntry = new TMEntry();
				for (int j = 0; j < tuvs.getLength(); j++) {
					var tuv = (Element) tuvs.item(j);
					var lang = tuv.getAttribute("xml:lang");
					if (lang.equals(sourceLang)) { // does it need to be culture invariant?
						var segments = tuv.getElementsByTagName("seg");
						if (segments.getLength() == 0)
							continue;
						tmEntry.source = XMLUtils.getInnerXMLString((Element) segments.item(0)).trim();
						// set the context
						var metadata = new HashMap<String, String>();
						var props = tuv.getElementsByTagName("prop");
						for (int k = 0; k < props.getLength(); k++) {
							var prop = (Element) props.item(k);
							if (prop.getAttribute("type").equals("x-context-pre"))
								metadata.put("prevSegment", prop.getTextContent());
							if (prop.getAttribute("type").equals("x-context-post"))
								metadata.put("nextSegment", prop.getTextContent());
						}
						if (metadata.keySet().size() > 0)
							tmEntry.metadata = gson.toJson(metadata);
					}
					if (lang.equals(targetLang)) {
						var segments = tuv.getElementsByTagName("seg");
						if (segments.getLength() == 0)
							continue;
						tmEntry.target = XMLUtils.getInnerXMLString((Element) segments.item(0)).trim();
					}
				}
				// check if the entry is valid
				if (!Util.isEmpty(tmEntry.source) && !Util.isEmpty(tmEntry.target))
					tmEntries.add(tmEntry);
			}

			// do the import
			int allItems = AddTMEntries(sTMName, tmEntries.toArray(new TMEntry[tmEntries.size()]));
			int itemsAfter = GetTMWriter(sTMName).getIndexWriter().numDocs();

			long lElapsedMillis = System.currentTimeMillis() - lStart;
			System.out.println("importTmx: " + lElapsedMillis + "ms.");
			if (lElapsedMillis > LOG_TIMEOUT)
				Logging.Log("TMErrors.log", "AddTMEntries completed in " + lElapsedMillis + "ms. TM name: " + sTMName);

			var result = new TMImportResult();
			result.allItems = allItems;
			result.importedItems = itemsAfter - itemsBefore;
			return result;

		} catch (Exception ex) {
			Logging.Log("TMEntries.log", "ERROR: import TM, name -> " + sTMName + "\n\n" + ex.toString());
			throw new FaultException(ex.getMessage());
		} finally {
		}
	}

	public String exportTmx(String sTMName) throws FaultException, IOException {
		throw new UnsupportedOperationException();
	}

	public int AddTMEntries(String sTMName, TMEntry[] tmEntries) throws FaultException {
		throw new UnsupportedOperationException();
	}

	public void AddTMEntry(String sTMName, TMEntry tmEntry) throws FaultException {
		AddTMEntries(sTMName, new TMEntry[] { tmEntry });
	}

	public void SetTMEntryCustomField(String sTMName, int key, String fieldName, Object value) throws FaultException {
		try {
			long lStart = System.currentTimeMillis();
			long lElapsedMillis = System.currentTimeMillis() - lStart;
			if (lElapsedMillis > LOG_TIMEOUT)
				Logging.Log("TMErrors.log",
						"SetTMEntryCustomField completed in " + lElapsedMillis + "ms. TM name: " + sTMName);
		} catch (Exception ex) {
			Logging.Log("TMEntries.log", "ERROR: " + ex.getMessage());
			throw new FaultException(ex.getMessage());
		}
	}

	public void updateTMEntry(String sTMName, int idEntry, String jsonData) throws FaultException {
		throw new UnsupportedOperationException();
	}

	public CorpusEntry[] Concordance(String[] aTMNames, String sSourceText, String sTargetText, boolean bCaseSensitive,
			boolean bNumericEquivalenve, int nLimit) {
		throw new UnsupportedOperationException();
	}

	public Date GetTMLastUpdated(String sTMName) {
		return null;
	}

	private org.w3c.dom.Node CreateAltTransNode(org.w3c.dom.Document xliff, String langFrom_ISO639_1,
			String langTo_ISO639_1, TMHit tmHit, String mid) {
		// create alt-trans element
		org.w3c.dom.Attr xmlAttribute = null;
		String nsUri = xliff.getDocumentElement().getNamespaceURI();
		org.w3c.dom.Element altTransNode = xliff.createElementNS(nsUri, "alt-trans");
		// mid
		if (!Util.isEmpty(mid)) {
			altTransNode.setAttribute("mid", mid);
		}
		// the quality
		altTransNode.setAttribute("match-quality", String.valueOf(tmHit.quality));
		// the origin
		altTransNode.setAttribute("origin", tmHit.origin);
		// okapi match type
		altTransNode.setAttribute("okp:matchType", tmHit.quality < 100 ? "FUZZY" : "EXACT");

		// source node
		var sourceNode = xliff.createElement("source");
		sourceNode.setAttribute("xml:lang", langFrom_ISO639_1);
		XmlUtils.setInnerXml(sourceNode, CATUtils.TextFragmentToXliff(tmHit.source));
		altTransNode.appendChild(sourceNode);

		// target node
		var targetNode = xliff.createElement("target");
		sourceNode.setAttribute("xml:lang", langTo_ISO639_1);
		XmlUtils.setInnerXml(targetNode, CATUtils.TextFragmentToXliff(tmHit.target));
		altTransNode.appendChild(targetNode);

		return altTransNode;
	}

	public String PreTranslateXliff(String sXliffContent, String langFrom_ISO639_1, String langTo_ISO639_1,
			TMAssignment[] aTMAssignments, int matchThreshold) throws FaultException {
		IQuery[] tmConnectors = null;
		try {
			// check the languages for the TMs
			if (aTMAssignments != null) {
				List<TMAssignment> lstTmpTMs = new ArrayList<TMAssignment>();
				for (TMAssignment tmAssignment : aTMAssignments) {
					var tmInfo = GetTMInfo(tmAssignment.TMName, false);
					if (tmInfo.langFrom.equals(langFrom_ISO639_1) && tmInfo.langTo.equals(langTo_ISO639_1))
						lstTmpTMs.add(tmAssignment);
				}
				aTMAssignments = lstTmpTMs.toArray(new TMAssignment[lstTmpTMs.size()]);
			} else
				aTMAssignments = new TMAssignment[0];

			// load the xliff file as xml
			DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
			factory.setNamespaceAware(false);
			// factory.setIgnoringElementContentWhitespace(false);
			DocumentBuilder builder = factory.newDocumentBuilder();
			var xliff = builder.parse(new ByteArrayInputStream(sXliffContent.getBytes(StandardCharsets.UTF_8)));
			// NamespaceContext nsctx = CATUtils.CreateNameSpaceContext(xliff, "x");
			// String nsUri = xliff.getDocumentElement().getNamespaceURI(); //
			// nsctx.getNamespaceURI("");

			// reset the source and target languages
			var fileNodes = xliff.getElementsByTagName("file");
			for (int i = 0; i < fileNodes.getLength(); i++) {
				var node = (Element) fileNodes.item(i);
				node.setAttribute("source-language", langFrom_ISO639_1);
				node.setAttribute("target-language", langTo_ISO639_1);
			}

			// Get the TM connectors. Disposable objects
			String[] aTMNames = Arrays.stream(aTMAssignments).map(x -> x.TMName).toArray(String[]::new);
			tmConnectors = GetTMConnectors(aTMNames);

			NodeList tuNodeList = xliff.getElementsByTagName("trans-unit");
			TextFragment prev = null;
			TextFragment next = null;
			for (int i = 0; i < tuNodeList.getLength(); i++) {
				long lStart = System.currentTimeMillis();
				Element tuNode = (Element) tuNodeList.item(i);

				// create the target node if it doesn't exists
				var targetNode = (Element) tuNode.getElementsByTagName("target").item(0);
				if (targetNode == null) {
					targetNode = xliff.createElement("target"); // xliff.createElementNS(nsUri, "target");
					// the target language
					targetNode.setAttribute("xml:lang", langTo_ISO639_1);
					tuNode.appendChild(targetNode);
				}

				// it is always segmented so there must be 'seg-source'
				Element sourceNode = (Element) tuNode.getElementsByTagName("seg-source").item(0);
				String targetXml = "";
				var sourceSegments = sourceNode.getElementsByTagName("mrk");
				var targetSegments = targetNode.getElementsByTagName("mrk");
				for (int j = 0; j < sourceSegments.getLength(); j++) {
					var sourceSegment = (Element) sourceSegments.item(j);
					Node status = sourceSegment.getAttributes().getNamedItem("status");
					if (status == null) {
						sourceSegment.setAttribute("status", ""); // just creates the attribute
						status = sourceSegment.getAttributes().getNamedItem("status");
					}

					String mid = sourceSegment.getAttribute("mid");
					// get the next segment for the context
					if (j < sourceSegments.getLength() - 1)
						next = CATUtils.XliffSegmentToTextFragmentSimple(
								XmlUtils.getInnerXMLString((Element) sourceSegments.item(j + 1)).trim());
					else {
						// pick the first segment from the next tu
						var ssNext = i < tuNodeList.getLength() - 1
								? ((Element) tuNodeList.item(i + 1)).getElementsByTagName("seg-source").item(0)
								: null;
						var nextSegment = ssNext != null ? ((Element) ssNext).getElementsByTagName("mrk").item(0)
								: null;
						next = nextSegment != null ? CATUtils.XliffSegmentToTextFragmentSimple(
								XmlUtils.getInnerXMLString((Element) nextSegment).trim()) : null;
					}

					Element targetSegment = null;
					if (targetSegments.getLength() > j)
						targetSegment = (Element) targetSegments.item(j);
					if (targetSegment == null || XmlUtils.getInnerXMLString(targetSegment).length() == 0) {
						var sSource = XmlUtils.getInnerXMLString(sourceSegment);
						String sStartingWhiteSpaces = Pattern.compile("^\\s*").matcher(sSource).results().findFirst()
								.get().group();
						String sEndingWhiteSpaces = Pattern.compile("\\s*$").matcher(sSource).results().findFirst()
								.get().group();
						var source = CATUtils.XliffSegmentToTextFragmentSimple(sSource.trim());
						var tmHits = QueryTM(tmConnectors, source, prev, next, (byte) matchThreshold, 1);
						// set the current segment as previous segment
						prev = source;

						// set the translation
						if (tmHits != null && tmHits.length > 0) {
							// fix me! The penalties
							var tmHit = tmHits[0];
							if (tmHit.quality >= 100) // pre-translate only with the exact matches.
							{
								var xlfTarget = CATUtils.TextFragmentToXliff(tmHit.target);
								targetXml += "<mrk mid=\"" + mid + "\" mtype=\"seg\">" + sStartingWhiteSpaces
										+ xlfTarget + sEndingWhiteSpaces + "</mrk>";
								status.setNodeValue("tmPreTranslated:" + tmHit.quality);
							} else {
								targetXml += "<mrk mid=\"" + mid + "\" mtype=\"seg\"></mrk>";
								status.setNodeValue("tmNotStarted");
							}

							// the alt-trans node
							Node altTransNode = CreateAltTransNode(xliff, langFrom_ISO639_1, langTo_ISO639_1, tmHit,
									mid);
							tuNode.appendChild(altTransNode);
						} else {
							targetXml += "<mrk mid=\"" + mid + "\" mtype=\"seg\"></mrk>";
							status.setNodeValue("tmNotStarted");
						}
					} else {
						targetXml += "<mrk mid=\"" + mid + "\" mtype=\"seg\">"
								+ XmlUtils.getInnerXMLString(targetSegment) + "</mrk>";
						status.setNodeValue("tmEdited");
					}
				}

				// set the target
				// targetNode.setNodeValue(targetXml);
				XmlUtils.setInnerXml(targetNode, targetXml);
				int a = 0;
			}

			String sRet = XmlUtils.DocumentToString(xliff).toString();

			// Logging.Log("PreTranslate.log", String.Format("End ... min: {0}ms, max:
			// {1}ms, avg: {2}ms total: {3}ms", min, max, total / tuNodeList.Count, total));

			return sRet;
		} catch (Exception ex) {
			Logging.Log("PreTranslate.log", "ERROR: " + ex.toString());
			throw new FaultException(ex.getMessage());
		} finally {
			// fix me! These are disposable object we could use 'using'
			// if (tmConnectors != null && tmConnectors.length > 0) {
			// for (IQuery tmConnector : tmConnectors)
			// tmConnector.close();
			// }
		}
	}

	public TMInfo GetTMInfo(String sTMName) throws FaultException {
		try {
			return GetTMInfo(sTMName, true);
		} catch (Exception ex) {
			Logging.Log("TMErrors.log", "ERROR: " + ex.getMessage());
			throw new FaultException(ex.getMessage());
		}
	}

	/// <summary>
	/// GetTMInfo
	/// </summary>
	/// <param name="sTMName"></param>
	/// <param name="reducedInfo"></param>
	/// <returns>TMInfo</returns>
	public TMInfo GetTMInfo(String sTMName, boolean fullInfo) throws FaultException {
		try {
			var tmInfo = new TMInfo();
			if (!TMExists(sTMName))
				return null;
			
			// get the languages from the TM name
			sTMName = FixTMName(sTMName);
			Optional<MatchResult> m = null;
			if (sTMName.startsWith("$__")) // global
				m = Pattern.compile("_(.+)_(.+)_(.+)\\.", Pattern.CASE_INSENSITIVE).matcher(sTMName).results()
						.findFirst();
			else if (sTMName.contains("_sec_")) // secondary
				m = Pattern.compile("_sec_(.+)_(.+)_(.+)\\.", Pattern.CASE_INSENSITIVE).matcher(sTMName).results()
						.findFirst();
			else // primary
				m = Pattern.compile("_(.+)_(.+)_(.+)\\.", Pattern.CASE_INSENSITIVE).matcher(sTMName).results()
						.findFirst();
			var langFrom_ISO639_1 = Constants.LANGUAGE_CODES.get(m.get().group(2));
			var lagngTo_ISO639_1 = Constants.LANGUAGE_CODES.get(m.get().group(3));

			tmInfo.name = sTMName;
			tmInfo.guid = "N/A";
			tmInfo.langFrom = langFrom_ISO639_1;
			tmInfo.langTo = lagngTo_ISO639_1;

			if (fullInfo) {
				// additional info
			}

			return tmInfo;
		} catch (Exception ex) {
			Logging.Log("TMEntries.log", "ERROR: " + ex.toString());
			throw new FaultException(ex.getMessage());
		}
	}

	public TMInfo[] getTMList(String sTMNameContains) throws Exception {
		// REPOSITORY_FOLDER
		try {
			// get the directory names for the TMs
			String sDirectory = REPOSITORY_FOLDER;

			File[] aTMPaths = null;
			if (Util.isEmpty(sTMNameContains))
				aTMPaths = new File(sDirectory).listFiles(File::isDirectory);
			else {
				aTMPaths = new File(sDirectory).listFiles(new FileFilter() {
					@Override
					public boolean accept(File file) {
						return file.getName().contains(sTMNameContains) && file.isDirectory();
					}
				});
			}

			var lstTMs = new ArrayList<TMInfo>();
			for (File tmPath : aTMPaths) {
				var sTMName = tmPath.getName();
				sTMName = FilenameUtils.removeExtension(sTMName); // remove .pemtm
				var matcher = Pattern.compile("(_*sec_|_*).*_(.*)_(.*)").matcher(sTMName);
				matcher.find();
				var tmInfo = new TMInfo();
				tmInfo.name = sTMName;
				tmInfo.guid = "N/A";
				tmInfo.langFrom = matcher.group(2);
				tmInfo.langTo = matcher.group(3);

				lstTMs.add(tmInfo);
			}
			return lstTMs.toArray(new TMInfo[lstTMs.size()]);
		} catch (Exception ex) {
			throw new FaultException(ex.getMessage());
		}
	}

	public void deleteTMEntry(String sTMName, int idEntry) throws Exception {
		throw new UnsupportedOperationException();
	}
}
