package com.tm.okapi.steps;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.StringReader;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.regex.Pattern;

import javax.xml.parsers.*;

import org.apache.axis.utils.XMLUtils;
import org.w3c.dom.*;

import com.tm.utils.CATUtils;

import net.sf.okapi.common.*;

//import com.sun.org.apache.xml.internal.utils.PrefixResolver;

import net.sf.okapi.common.LocaleId;
import net.sf.okapi.common.pipeline.BasePipelineStep;
import net.sf.okapi.common.resource.ISegments;
import net.sf.okapi.common.resource.ITextUnit;
import net.sf.okapi.common.resource.Segment;
import net.sf.okapi.common.resource.TextContainer;
import net.sf.okapi.common.resource.TextFragment;

public class XliffTranslationStep extends BasePipelineStep {
	private LocaleId sourceLocale;
	private LocaleId targetLocale;
	private int segmentCounter = 0;
	private List<NodeList> translatedSegments;
	/*
	 * private final PrefixResolver resolver = new
	 * PrefixResolverDefault(doc.getDocumentElement()); NamespaceContext ctx = new
	 * NamespaceContext() { public String getNamespaceURI(String prefix) { return
	 * resolver.getNamespaceForPrefix(prefix); }
	 *
	 * // Dummy implementation - not used! public Iterator getPrefixes(String val) {
	 * return null; }
	 *
	 * // Dummy implemenation - not used! public String getPrefix(String uri) {
	 * return null; } };
	 */

	public XliffTranslationStep(String sXliffContent) throws Exception {
		// open the tarnslated xliff
		DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
		factory.setNamespaceAware(false);
		DocumentBuilder builder = factory.newDocumentBuilder();
		Document xliff = builder.parse(new ByteArrayInputStream(sXliffContent.getBytes(StandardCharsets.UTF_8)));
		NodeList tus = xliff.getElementsByTagName("trans-unit");
		translatedSegments = new ArrayList<NodeList>();
		for (int i = 0; i < tus.getLength(); i++) {
			Element tu = (Element) tus.item(i);
			NodeList lstTarget = tu.getElementsByTagName("target");
			Element target = (Element) lstTarget.item(0);
			NodeList segments = target.getElementsByTagName("mrk");
			if (segments.getLength() == 0)
				translatedSegments.add(lstTarget);
			else
				translatedSegments.add(segments);
		}
	}

	public void setSourceLocale(LocaleId sourceLocale) {
		this.sourceLocale = sourceLocale;
	}

	public void setTargetLocale(LocaleId targetLocale) {
		this.targetLocale = targetLocale;
	}

	@Override
	public String getName() {
		return "TM Translation step";
	}

	@Override
	public String getDescription() {
		return "Translates text units content from xliff.";
	}

	@Override
	protected Event handleStartDocument(Event theEvent) {
		segmentCounter = 0;

		return super.handleStartDocument(theEvent);
	}

	@Override
	protected Event handleTextUnit(Event theEvent) {
		try {
			ITextUnit tu = theEvent.getTextUnit();
			if (!tu.isTranslatable())
				return super.handleTextUnit(theEvent);

			ISegments sourceSegments = tu.getSourceSegments();// tu.getSource().getSegments();
			TextContainer targetTextContainer = tu.createTarget(targetLocale, false, IResource.COPY_SEGMENTATION);
			ISegments targetSegments = targetTextContainer.getSegments();

			for (int i = 0; i < sourceSegments.count(); i++) {
				// get the segments
				Segment sourceSegment = (Segment) sourceSegments.get(i);
				Segment targetSegment = targetSegments.get(sourceSegment.getId());
				if (targetSegment == null) {
					targetSegment = new Segment();
					targetSegments.append(targetSegment);
				}
				String sStartingWhiteSpaces = Pattern.compile("^\\s*").matcher(sourceSegment.text).results().findFirst()
						.get().group();
				String sEndingWhiteSpaces = Pattern.compile("\\s*$").matcher(sourceSegment.text).results().findFirst()
						.get().group();

				// set the translation
				TextFragment tfTranslation = null;
				Element translationNode = (Element) translatedSegments.get(segmentCounter).item(i);
				if (Util.isEmpty(translationNode.getTextContent()))
					tfTranslation = new TextFragment("");
				else
					tfTranslation = CATUtils.XliffSegmentToTextFragment(
							sStartingWhiteSpaces + XMLUtils.getInnerXMLString(translationNode).trim() + sEndingWhiteSpaces);

				tfTranslation.setCodedText(tfTranslation.getCodedText(), sourceSegment.getContent().getClonedCodes());
				targetSegment.setContent(tfTranslation);
			}
			segmentCounter++;
		} catch (Exception ex) {
			System.out.println("ERROR: " + ex.toString());
			throw ex;
		}

		return super.handleTextUnit(theEvent);
	}
}
