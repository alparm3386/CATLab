package com.tm.okapi.filters.regextagger;

import com.tm.okapi.filters.DefaultFilters;
import net.sf.okapi.common.IParameters;
import net.sf.okapi.common.ParametersDescription;
import net.sf.okapi.common.StringParameters;
import net.sf.okapi.common.Util;
import net.sf.okapi.common.exceptions.OkapiException;
import net.sf.okapi.common.exceptions.OkapiNotImplementedException;
import net.sf.okapi.common.filters.FilterConfigurationMapper;
import net.sf.okapi.common.filters.IFilter;
import net.sf.okapi.common.filterwriter.IFilterWriter;
import org.w3c.dom.*;
import org.xml.sax.InputSource;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.StringReader;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Base64;
import java.util.List;

public class Parameters implements IParameters {
    public Parameters () {
        super();
    }
    private List<IFilter> filterChain;

    @Override
    public void reset() {
        filterChain.clear();
    }

    public List<IFilter> getFilterChain() {
        return filterChain;
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
        }
        catch (Exception e)
        {
            if ( !ignoreErrors )
                throw new OkapiException(e);
        }
    }

    @Override
    public void save(String filePath) {
        throw new OkapiNotImplementedException();
    }

    @Override
    public String getPath() {
        throw new OkapiNotImplementedException();
    }

    @Override
    public void setPath(String filePath) {
        throw new OkapiNotImplementedException();
    }

    @Override
    public boolean getBoolean(String name) {
        throw new OkapiNotImplementedException();
    }

    @Override
    public void setBoolean(String name, boolean value) {
        throw new OkapiNotImplementedException();
    }

    @Override
    public String getString(String name) {
        throw new OkapiNotImplementedException();
    }

    @Override
    public void setString(String name, String value) {
        throw new OkapiNotImplementedException();
    }

    @Override
    public int getInteger(String name) {
        throw new OkapiNotImplementedException();
    }

    @Override
    public void setInteger(String name, int value) {
        throw new OkapiNotImplementedException();
    }

    @Override
    public ParametersDescription getParametersDescription() {
        return null;
    }
}
