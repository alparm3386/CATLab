package com.tm.okapi.utils;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.*;
import java.util.regex.MatchResult;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import javax.xml.parsers.*;
import javax.xml.xpath.*;

//import org.apache.axis.utils.XMLUtils;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;

import com.tm.okapi.service.TMAssignment;

import net.sf.okapi.common.*;
import net.sf.okapi.common.filters.IFilter;
import net.sf.okapi.common.filterwriter.TMXContent;
import net.sf.okapi.common.filterwriter.XLIFFContent;
import net.sf.okapi.common.resource.*;
import net.sf.okapi.filters.tmx.TmxUtils;
import net.sf.okapi.filters.xliff.XLIFFFilter;

public class CATUtils {
	public static class RegEx {
		public static MatchResult match(String input, String pattern) {
			return Pattern.compile(pattern).matcher(input).results().findFirst().get();
		}
	}

	/// <summary>
	/// TextFragmentToXLIFF
	/// </summary>
	/// <param name="fragment"></param>
	/// <returns></returns>
	public static String TextFragmentToXLIFF(TextFragment fragment) {
		if (fragment == null) {
			return "";
		}

		XLIFFContent fmt = new XLIFFContent();
		fmt.setContent(fragment);
		return fmt.toString();
	}

	/// <summary>
	/// XliffSegmentToTextFragment
	/// </summary>
	/// <param name="sXliffSegment"></param>
	/// <returns></returns>
	public static TextFragment XliffSegmentToTextFragment(String sXliffSegment) {
		IFilter filter = null;
		try {

			// fix me! There must be a simpler way
			// create xliff from the segment
			var sTmpXliff = "<xliff xmlns='urn:oasis:names:tc:xliff:document:1.2'><trans-unit id='1'><source>"
					+ sXliffSegment + "</source></trans-unit></xliff>";

			InputStream input = new ByteArrayInputStream(sTmpXliff.getBytes(StandardCharsets.UTF_8));
			RawDocument res = new RawDocument(input, "UTF-8", LocaleId.fromString("en"), LocaleId.fromString("en"));

			filter = new XLIFFFilter();

			var xliffParams = (net.sf.okapi.filters.xliff.Parameters) filter.getParameters();
			// xliffParams.setUseCustomParser(false); //woodstox problem
			xliffParams.setPreserveSpaceByDefault(true);
			filter.open(res);

			while (filter.hasNext()) {
				Event ev = filter.next();
				if (ev.getEventType() == EventType.TEXT_UNIT) {
					// Check if this unit can be modified
					var tu = (TextUnit) ev.getResource();
					// if (!tu.isTranslatable())
					// continue;

					var tcSource = tu.getSource();
					return tcSource.getFirstContent(); // there must be only one text fragment
				}
			}
		} catch (Exception ex) {

		} finally {
			if (filter != null)
				filter.close();
		}

		throw new NullPointerException("Unable to convert to TextFragment");
	}

	public static TextFragment XliffSegmentToTextFragmentSimple(String sXliffSegment) {
		try {
			// remove the outer tag
			StringBuilder sbOut = new StringBuilder();

			// matcher for the tmx tags
			Pattern pattern = Pattern.compile("<[^>]*>[^>]*>");
			Matcher matcher = pattern.matcher(sXliffSegment);
			int tagCntr = 0;
			int prevEnd = 0;
			String sText = "";
			while (matcher.find()) {
				String sTag = matcher.group().toLowerCase();
				sText = sXliffSegment.substring(prevEnd, matcher.start());
				prevEnd = matcher.end();
				sbOut.append(sText);
				if (sTag.startsWith("<bpt")) {
					sbOut.append("" + ((char) TextFragment.MARKER_OPENING) + (char) (tagCntr + TextFragment.CHARBASE));
				} else if (sTag.startsWith("<ept")) {
					sbOut.append("" + ((char) TextFragment.MARKER_CLOSING) + (char) (tagCntr + TextFragment.CHARBASE));
				} else if (sTag.startsWith("<ph") || sTag.startsWith("<x")) {
					sbOut.append("" + ((char) TextFragment.MARKER_ISOLATED) + (char) (tagCntr + TextFragment.CHARBASE));
				}

				tagCntr++;
			}

			sText = sXliffSegment.substring(prevEnd, sXliffSegment.length());
			sbOut.append(sText);

			return new TextFragment(sbOut.toString());
		} catch (Exception ex) {
			throw new NullPointerException("XliffSegmentToTextFragmentSimple");
		} finally {
		}
	}

	/// <summary>
	/// XliffTags2TMXTags
	/// </summary>
	/// <param name="sSource"></param>
	/// <returns></returns>
	public static String XliffTags2TMXTags(String sSource) {
		String sConverted = sSource;
		sConverted = sConverted.replaceAll("<x id=['\"].*?['\"].*?/>", "<ph type='fmt'>{}</ph>"); // no x element in TMX
		sConverted = sConverted.replaceAll("(?<=<[^>]*?)(id)", "i"); // regex seems to be faster than xslt
		sConverted = sConverted.replaceAll("(?<=<[^>]*?)(ctype)", "type");
		sConverted = sConverted.replaceAll("(?<=<[^>]*?)(type=\"underlined\")", "type=\"ulined\"");
		sConverted = sConverted.replaceAll(" xmlns=\"urn:oasis:names:tc:xliff:document:1.2\"", ""); // is there better
																									// solution?

		return sConverted;
	}

	/*
	 * public static NamespaceContextImpl
	 * CreateNameSpaceContext(org.w3c.dom.Document doc, String defaultPrefix) {
	 * 
	 * final PrefixResolver resolver = new
	 * PrefixResolverDefault(doc.getDocumentElement()); NamespaceContextImpl ctx =
	 * new NamespaceContext() { public String getNamespaceURI(String prefix) {
	 * if("x".equals(prefix)) return resolver.getNamespaceForPrefix("");
	 * 
	 * return resolver.getNamespaceForPrefix(prefix); }
	 * 
	 * // Dummy implementation - not used! public Iterator getPrefixes(String val) {
	 * return null; }
	 * 
	 * // Dummy implemenation - not used! public String getPrefix(String uri) {
	 * return null; }
	 * 
	 * };
	 *
	 * return ctx;}
	 */

	/// <summary>
	/// TextFragmentToXLIFF
	/// </summary>
	/// <param name="fragment"></param>
	/// <returns></returns>
	public static String TextFragmentToTmx(TextFragment fragment) {
		if (fragment == null)
			return "";

		TMXContent fmt = new TMXContent();
		fmt.setContent(fragment);
		return fmt.toString();
	}

	/// <summary>
	/// TextFragmentToXLIFF
	/// </summary>
	/// <param name="fragment"></param>
	/// <returns></returns>
	public static String TextFragmentToXliff(TextFragment fragment) {
		if (fragment == null)
			return "";

		XLIFFContent fmt = new XLIFFContent();
		fmt.setContent(fragment);
		return fmt.toString();
	}
}
