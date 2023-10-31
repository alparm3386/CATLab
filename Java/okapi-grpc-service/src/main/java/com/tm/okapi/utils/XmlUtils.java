package com.tm.okapi.utils;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.OutputStream;
import java.io.StringReader;
import java.io.UnsupportedEncodingException;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.*;

import org.apache.axis.utils.XMLUtils;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.xml.sax.InputSource;

import net.sf.okapi.common.XMLWriter;

public class XmlUtils extends XMLUtils {
	/**
	 * @param target
	 * @param innerXml
	 */
	public static void setInnerXml(Element target, String innerXml) {
		// delete children
		var oldChildren = target.getChildNodes();
		for (int i = 0; i < oldChildren.getLength(); i++) {
			var oldChild = oldChildren.item(i);
			target.removeChild(oldChild);
		}
		Element tmpElement = null;
		try {
			tmpElement = DocumentBuilderFactory.newInstance().newDocumentBuilder()
					.parse(new InputSource(new StringReader("<root>" + innerXml + "</root>"))).getDocumentElement();
		} catch (Exception e) {
			e.printStackTrace();
		}

		// copy the new children
		var newChildren = tmpElement.getChildNodes();
		for (int i = 0; i < newChildren.getLength(); i++) {
			var newChild = newChildren.item(i);
			var imported = target.getOwnerDocument().importNode(newChild, true);
			target.appendChild(imported);
		}
	}

	public static void writeXml(Document doc, String xmlFilePath)
			throws TransformerException, UnsupportedEncodingException {

        // create the xml file
        //transform the DOM Object to an XML File
        TransformerFactory transformerFactory = TransformerFactory.newInstance();
        Transformer transformer = transformerFactory.newTransformer();
        DOMSource domSource = new DOMSource(doc);
        StreamResult streamResult = new StreamResult(new File(xmlFilePath).toURI().getPath());

        // If you use
        // StreamResult result = new StreamResult(System.out);
        // the output will be pushed to the standard output ...
        // You can use that for debugging 

        transformer.transform(domSource, streamResult);
	}

	public static void writeXml(Document doc, ByteArrayOutputStream outStream)
			throws TransformerException, UnsupportedEncodingException {

        // create the xml file
        //transform the DOM Object to an XML File
        TransformerFactory transformerFactory = TransformerFactory.newInstance();
        Transformer transformer = transformerFactory.newTransformer();
        DOMSource domSource = new DOMSource(doc);
        StreamResult streamResult = new StreamResult(outStream);

        // If you use
        // StreamResult result = new StreamResult(System.out);
        // the output will be pushed to the standard output ...
        // You can use that for debugging 

        transformer.transform(domSource, streamResult);
	}
}
