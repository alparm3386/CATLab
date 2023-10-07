package com.tm.okapi.filters.cascading;

import com.tm.okapi.utils.StringUtils;
import net.sf.okapi.common.exceptions.OkapiException;
import org.apache.axis.utils.XMLUtils;
import org.eclipse.swt.widgets.Dialog;
import org.eclipse.swt.widgets.Display;
import org.eclipse.swt.widgets.Shell;
import org.eclipse.swt.browser.Browser;
import org.eclipse.swt.SWT;
import org.w3c.dom.*;
import org.w3c.dom.ls.*;
import org.xml.sax.SAXException;

import javax.xml.bind.annotation.XmlElement;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import java.io.ByteArrayInputStream;
import java.io.File;
import java.io.IOException;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.regex.Pattern;

public class XliffViewer extends Dialog {

    protected Object result;
    protected Shell shell;
    private Browser browser;

    /**
     * Create the dialog.
     *
     * @param parent
     * @param style
     */
    public XliffViewer(Shell parent, int style) {
        super(parent, style);
        setText("Viewer");
    }

    /**
     * Open the dialog.
     *
     * @return the result
     */
    public Object open(String xliffPath, boolean sourceOnly) {
        createContents();
        shell.open();
        shell.layout();

        //set the html
        var sHtml = xliffToHtml(xliffPath, sourceOnly);
        browser.setText(sHtml);

        Display display = getParent().getDisplay();
        while (!shell.isDisposed()) {
            if (!display.readAndDispatch()) {
                display.sleep();
            }
        }
        return result;
    }

    public static String xmlTags2GoogleTags(Element xliffSegment) {
        try {
            var sRet = new StringBuilder();
            var nodeList = xliffSegment.getChildNodes();
            var openTags = new HashMap<String, String>();
            int id = 1;
            //String outerXml = "";
            for (int i = 0; i < nodeList.getLength(); i++) {
                var node = nodeList.item(i);
                //outerXml = com.tm.okapi.utils.XmlUtils.getOuterXml(node);
                if (node.getNodeType() == Node.TEXT_NODE)
                    sRet.append(node.getTextContent());
                else if (node.getNodeType() ==  Node.ELEMENT_NODE) {
                    switch (node.getNodeName()) {
                        case "ph":
                        case "x":
                            sRet.append("{" + id + "/}");
                            id++;
                            break;
                        case "bpt":
                            sRet.append("{" + id + "}");
                            openTags.put(node.getAttributes().getNamedItem("id").getNodeValue(), String.valueOf(id));
                            id++;
                            break;
                        case "ept":
                            sRet.append("{/" + openTags.get(node.getAttributes().getNamedItem("id").getNodeValue()) + "}");
                            break;
                        case "it":
                            sRet.append("{" + id + "/}");
                            id++;
                            break;
                        default:
                            break;
                    }
                }
            }

            return sRet.toString();
        } catch (Exception ex) {
            throw new OkapiException("id=??");
        }
    }

    private String xliffToHtml(String xliffPath, boolean sourceOnly) {
        try {
            //open the xliff
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(false);
            DocumentBuilder builder = factory.newDocumentBuilder();
            var xliff = builder.parse(new File(xliffPath));
            var sbHtml = new StringBuilder();
            var tus = xliff.getElementsByTagName("trans-unit");
            for (int i = 0; i < tus.getLength(); i++) {
                var tu = (Element) tus.item(i);
                var lstSource = tu.getElementsByTagName("seg-source");
                var source = (Element) lstSource.item(0);
                var segments = source.getElementsByTagName("mrk");
                var srcGoggle = "";
                if (segments.getLength() == 0) {
                    srcGoggle = xmlTags2GoogleTags(source).trim();
                    srcGoggle = StringUtils.encodeHtml(srcGoggle);
                    srcGoggle = srcGoggle.replaceAll("(\\{\\/*\\d*\\/*})", "<span style=\"color:red\">$1</span>");
                    sbHtml.append("<div>" + srcGoggle + "</div><hr/>\r\n");
                } else {
                    for (int j = 0; j < segments.getLength(); j++) {
                        var seg = (Element) segments.item(j);
                        srcGoggle = xmlTags2GoogleTags(seg).trim();
                        srcGoggle = StringUtils.encodeHtml(srcGoggle);
                        srcGoggle = srcGoggle.replaceAll("(\\{\\/*\\d*\\/*})", "<span style=\"color:red\">$1</span>");
                        sbHtml.append("<div>" + srcGoggle + "</div><hr/>\r\n");
                    }
                }
            }

            return "<div>\r\n" + sbHtml.toString() + "\r\n</div>";
        } catch (Exception ex) {
            throw new OkapiException(ex);
        }
    }

    /**
     * Create contents of the dialog.
     */
    private void createContents() {
        shell = new Shell(getParent(), getStyle());
        shell.setSize(800, 600);
        shell.setText(getText());

        browser = new Browser(shell, SWT.NONE);
        browser.setBounds(0, 10, 785, 580);
    }
}
