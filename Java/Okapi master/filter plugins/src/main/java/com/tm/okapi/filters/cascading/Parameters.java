package com.tm.okapi.filters.cascading;

import com.tm.okapi.filters.DefaultFilters;
import net.sf.okapi.common.IParameters;
import net.sf.okapi.common.ParametersDescription;
import net.sf.okapi.common.StringParameters;
import net.sf.okapi.common.Util;
import net.sf.okapi.common.exceptions.OkapiException;
import net.sf.okapi.common.filters.FilterConfigurationMapper;
import net.sf.okapi.common.filters.IFilter;
import net.sf.okapi.common.filterwriter.IFilterWriter;
import org.apache.commons.io.FileUtils;
import org.w3c.dom.*;
import org.xml.sax.InputSource;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;
import java.io.*;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Base64;
import java.util.List;

public class Parameters extends StringParameters {
    private List<IFilter> filterChain;
    private List<String> configIds;
    private String path;

    public Parameters () {
        super();
        filterChain = new ArrayList<IFilter>();
        configIds = new ArrayList<String>();
    }

    @Override
    public void reset() {
        if (filterChain != null)
            filterChain.clear();
    }

    public List<IFilter> getFilterChain() {
        return filterChain;
    }
    public List<String> getConfigIds() {
        return configIds;
    }

    @Override
    public void fromString(String data) {
        InputStream stream = new ByteArrayInputStream(data.getBytes(StandardCharsets.UTF_8));
        load(stream, false);
    }

    @Override
    public void load(URL inputURL, boolean ignoreErrors) {
        try {
            var is = inputURL.openStream();
            load(is, ignoreErrors);
        } catch (IOException e) {
            if ( !ignoreErrors )
                throw new OkapiException(e);
        }
    }

    @Override
    public void load(InputStream inStream, boolean ignoreErrors) {
        try
        {
            //Create DocumentBuilder with default configuration
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            DocumentBuilder builder = factory.newDocumentBuilder();
            Document doc = builder.parse(new InputSource(inStream));
            var filterElement = (Element)doc.getElementsByTagName("filter").item(0);

            //create the filter chain
            filterChain = new ArrayList<IFilter>();
            configIds = new ArrayList<String>();
            CreateFilterFromXmlElement(filterElement);
        }
        catch (Exception e)
        {
            if ( !ignoreErrors )
                throw new OkapiException(e);
        }
    }

    private void CreateFilterFromXmlElement(Element filterElement) throws Exception {
        if (filterElement == null)
            return;
        //create the filter
        var configId = filterElement.getElementsByTagName("configId").item(0).getTextContent();
        var filter = DefaultFilters.createFilter(configId);
        XPath xpath = XPathFactory.newInstance().newXPath();

        //add the filter to the chain
        filterChain.add(filter);
        configIds.add(configId);

        //load the parameters
        var paramsElement = (Element)xpath.compile("params").evaluate(filterElement, XPathConstants.NODE);
        //var bodyElement = filterElement.getElementsByTagName("params").item(0);
        if (paramsElement != null) {
            //load the parameters
            var filterParams = paramsElement.getTextContent().trim();
            if (!Util.isEmpty(filterParams)) {
                byte[] decodedBytes = Base64.getDecoder().decode(filterParams);
                filterParams = new String(decodedBytes);
                var filterParam = filter.getParameters();
                var is = new ByteArrayInputStream(filterParams.getBytes());
                filterParam.load(is, false);
                filter.setParameters(filterParam);
            }
        }

        //check the sub-filter
        //var subFilterElement = filterElement.getElementsByTagName("sub-filter").item(0);
        var subFilterElement = (Element)xpath.compile("sub-filter").evaluate(filterElement, XPathConstants.NODE);
        if (subFilterElement != null) {
            CreateFilterFromXmlElement((Element)subFilterElement);
        }
    }

    @Override
    public String toString() {
        try {
            //if (filterChain.size() == 0)
            //    throw new OkapiException("Empty filter.");

            //Create DocumentBuilder with default configuration
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            DocumentBuilder builder = factory.newDocumentBuilder();
            Document doc = builder.newDocument();

            Element parentFilter = null;
            for (int i = 0; i < filterChain.size(); i++) {
                var filter = filterChain.get(i);
                var configId = configIds.get(i);
                // the filter element
                Element filterNode = null;
                if (parentFilter == null) {
                    filterNode = doc.createElement("filter");
                    doc.appendChild(filterNode);
                    parentFilter = filterNode;
                } else {
                    filterNode = doc.createElement("sub-filter");
                    parentFilter.appendChild(filterNode);
                    parentFilter = filterNode;
                }

                //configId
                var configIdNode = doc.createElement("configId");
                filterNode.appendChild(configIdNode);
                configIdNode.appendChild(doc.createTextNode(configId));
                //params
                var paramsNode = doc.createElement("params");
                filterNode.appendChild(paramsNode);
                var params = filter.getParameters().toString();
                byte[] encodedBytes = Base64.getEncoder().encode(params.getBytes(StandardCharsets.UTF_8));
                var encodedParams = new String(encodedBytes);
                paramsNode.appendChild(doc.createTextNode(encodedParams));
            }

            // create the xml file
            //transform the DOM Object to an XML File
            var transformerFactory = TransformerFactory.newInstance();
            var transformer = transformerFactory.newTransformer();
            var domSource = new DOMSource(doc);
            StringWriter writer = new StringWriter();
            var streamResult = new StreamResult(writer);

            // If you use
            // StreamResult result = new StreamResult(System.out);
            // the output will be pushed to the standard output ...
            // You can use that for debugging

            transformer.transform(domSource, streamResult);
            String strResult = writer.toString();
            if (filterChain.size() == 0) //empty filter
                strResult += "<filter-empty/>"; //otherwise, it will be invalid xml

            return strResult;
        } catch (Exception ex) {
            throw new OkapiException(ex);
        }
    }

    @Override
    public void save(String filePath) {
        try {
            var sContent = toString();
            //FileUtils.writeStringToFile(new File(filePath), sContent, "UTF-8");
            Files.write(Paths.get(filePath), sContent.getBytes("UTF-8"));
        } catch (Exception ex) {
            throw new OkapiException(ex);
        }
    }

    @Override
    public String getPath() {
        return path;
    }

    @Override
    public void setPath(String filePath) {
        path = filePath;
    }

    @Override
    public boolean getBoolean(String name) {
        return super.getBoolean(name);
    }

    @Override
    public void setBoolean(String name, boolean value) {
        super.setBoolean(name, value);
    }

    @Override
    public String getString(String name) {
        return super.getString(name);
    }

    @Override
    public void setString(String name, String value) {
        super.setString(name, value);
    }

    @Override
    public int getInteger(String name) {
        return super.getInteger(name);
    }

    @Override
    public void setInteger(String name, int value) {
        super.setInteger(name, value);
    }

    @Override
    public ParametersDescription getParametersDescription() {
        return null;
    }
}
