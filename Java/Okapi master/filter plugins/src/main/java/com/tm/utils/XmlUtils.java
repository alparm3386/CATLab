package com.tm.utils;

import java.io.*;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.*;

import net.sf.okapi.common.exceptions.OkapiException;
import org.apache.axis.utils.XMLUtils;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.w3c.dom.ls.DOMImplementationLS;
import org.w3c.dom.ls.LSSerializer;
import org.xml.sax.InputSource;

import net.sf.okapi.common.XMLWriter;

public class XmlUtils extends XMLUtils {
	/**
	 * @param target
	 * @param innerXml
	 */
	public static void setInnerXml(Element target, String innerXml) {
		// delete children
		NodeList oldChildren = target.getChildNodes();
		for (int i = 0; i < oldChildren.getLength(); i++) {
			Node oldChild = oldChildren.item(i);
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
		NodeList newChildren = tmpElement.getChildNodes();
		for (int i = 0; i < newChildren.getLength(); i++) {
			Node newChild = newChildren.item(i);
			Node imported = target.getOwnerDocument().importNode(newChild, true);
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

	public static String getInnerXml(Node node) {
		DOMImplementationLS lsImpl = (DOMImplementationLS) node.getOwnerDocument().getImplementation().getFeature("LS", "3.0");
		LSSerializer lsSerializer = lsImpl.createLSSerializer();
		lsSerializer.getDomConfig().setParameter("xml-declaration", false);
		NodeList childNodes = node.getChildNodes();
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < childNodes.getLength(); i++) {
			sb.append(lsSerializer.writeToString(childNodes.item(i)));
		}
		return sb.toString();
	}

	public static String getOuterXml(Node node) {
		try {
			Transformer transformer = TransformerFactory.newInstance().newTransformer();
			transformer.setOutputProperty("omit-xml-declaration", "yes");

			StringWriter writer = new StringWriter();
			transformer.transform(new DOMSource(node), new StreamResult(writer));
			return writer.toString();
		} catch (Exception ex) {
			throw new OkapiException(ex);
		}
	}
}
