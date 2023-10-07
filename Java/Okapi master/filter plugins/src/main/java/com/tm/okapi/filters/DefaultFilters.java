package com.tm.okapi.filters;

import net.sf.okapi.common.exceptions.OkapiException;
import net.sf.okapi.common.filters.*;

import java.util.Hashtable;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class DefaultFilters {
    private static Hashtable<String, String> filterMap;

    static {
        filterMap = new Hashtable<String, String>();
        filterMap.put("okf_dtd", "net.sf.okapi.filters.dtd.DTDFilter");
        filterMap.put("okf_html", "net.sf.okapi.filters.html.HtmlFilter");
        filterMap.put("okf_xliff", "net.sf.okapi.filters.xliff.XLIFFFilter");
        filterMap.put("okf_openxml", "net.sf.okapi.filters.openxml.OpenXMLFilter");
        filterMap.put("okf_openoffice", "net.sf.okapi.filters.openoffice.OpenOfficeFilter");
        filterMap.put("okf_odf", "net.sf.okapi.filters.openoffice.ODFFilter");
        filterMap.put("okf_properties", "net.sf.okapi.filters.properties.PropertiesFilter");
        filterMap.put("okf_po", "net.sf.okapi.filters.po.POFilter");
        filterMap.put("okf_regex", "net.sf.okapi.filters.regex.RegexFilter");
        filterMap.put("okf_ts", "net.sf.okapi.filters.ts.TsFilter");
        filterMap.put("okf_plaintext", "net.sf.okapi.filters.plaintext.PlainTextFilter");
        filterMap.put("okf_table", "net.sf.okapi.filters.table.TableFilter");
        filterMap.put("okf_tmx", "net.sf.okapi.filters.tmx.TmxFilter");
        filterMap.put("okf_xml", "net.sf.okapi.filters.xml.XMLFilter");
        filterMap.put("okf_idml", "net.sf.okapi.filters.idml.IDMLFilter");
        filterMap.put("okf_json", "net.sf.okapi.filters.json.JSONFilter");
        filterMap.put("okf_phpcontent", "net.sf.okapi.filters.php.PHPContentFilter");
        filterMap.put("okf_ttx", "net.sf.okapi.filters.ttx.TTXFilter");
        filterMap.put("okf_pensieve", "net.sf.okapi.filters.pensieve.PensieveFilter");
        filterMap.put("okf_vignette", "net.sf.okapi.filters.vignette.VignetteFilter");
        filterMap.put("okf_yaml", "net.sf.okapi.filters.yaml.YamlFilter");
        filterMap.put("okf_xmlstream", "net.sf.okapi.filters.xmlstream.XmlStreamFilter");
        filterMap.put("okf_tradosrtf", "net.sf.okapi.filters.rtf.RTFFilter");
        filterMap.put("okf_mosestext", "net.sf.okapi.filters.mosestext.MosesTextFilter");
        filterMap.put("okf_mif", "net.sf.okapi.filters.mif.MIFFilter");
        filterMap.put("okf_rainbowkit", "net.sf.okapi.filters.rainbowkit.RainbowKitFilter");
        filterMap.put("okf_transifex", "net.sf.okapi.filters.transifex.TransifexFilter");
        filterMap.put("okf_archive", "net.sf.okapi.filters.archive.ArchiveFilter");
        filterMap.put("okf_xini", "net.sf.okapi.filters.xini.XINIFilter");
        filterMap.put("okf_txml", "net.sf.okapi.filters.txml.TXMLFilter");
        filterMap.put("okf_itshtml5", "net.sf.okapi.filters.its.html5.HTML5Filter");
        filterMap.put("okf_doxygen", "net.sf.okapi.filters.doxygen.DoxygenFilter");
        filterMap.put("okf_wiki", "net.sf.okapi.filters.wiki.WikiFilter");
        filterMap.put("okf_simplification", "net.sf.okapi.lib.preprocessing.filters.simplification.SimplificationFilter");
        filterMap.put("okf_transtable", "net.sf.okapi.filters.transtable.TransTableFilter");
        filterMap.put("okf_icml", "net.sf.okapi.filters.icml.ICMLFilter");
        filterMap.put("okf_xliff2", "net.sf.okapi.filters.xliff2.XLIFF2Filter");
        filterMap.put("okf_markdown", "net.sf.okapi.filters.markdown.MarkdownFilter");
        filterMap.put("okf_sdlpackage", "net.sf.okapi.filters.sdlpackage.SdlPackageFilter");
        filterMap.put("okf_autoxliff", "net.sf.okapi.filters.autoxliff.AutoXLIFFFilter");
        filterMap.put("okf_tex", "net.sf.okapi.filters.tex.TEXFilter");
        filterMap.put("okf_multiparsers", "net.sf.okapi.filters.multiparsers.MultiParsersFilter");
        filterMap.put("okf_cascading", "com.tm.okapi.filters.cascading.CascadingFilter");
    }

    public static IFilter createFilter(String id) {
        try {
            String className = filterMap.get(id);
            IFilter filter = (IFilter) Class.forName(className).newInstance();

            return filter;
        } catch (Exception ex) {
            throw  new OkapiException(ex.getMessage());
        }
    }

    public static IFilter createFilterByFilterName(String filterName) {
        try {
            Pattern pattern = Pattern.compile("(.*)@", Pattern.CASE_INSENSITIVE);
            Matcher matcher = pattern.matcher(filterName);
            matcher.find();
            String id = matcher.group(1);
            String className = filterMap.get(id);
            IFilter filter = (IFilter) Class.forName(className).newInstance();

            return filter;
        } catch (Exception ex) {
            throw  new OkapiException(ex.getMessage());
        }
    }

}
