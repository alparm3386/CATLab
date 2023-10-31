package com.tm.okapi.filters.regextagger;

import net.sf.okapi.common.*;
import net.sf.okapi.common.encoder.EncoderManager;
import net.sf.okapi.common.exceptions.OkapiIOException;
import net.sf.okapi.common.filters.*;
import net.sf.okapi.common.filterwriter.IFilterWriter;
import net.sf.okapi.common.resource.RawDocument;
import net.sf.okapi.common.skeleton.ISkeletonWriter;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.*;

public class RegexTagger implements IFilter {

    private LocaleId srcLoc;
    private List<IFilter> filterChain;
    private boolean cancelled = false;
    private LinkedList<Event> events;
    private IFilterConfigurationMapper fcMapper;
    private String mimeType = "application/octet-stream";
    private final Logger logger = LoggerFactory.getLogger(getClass());
    private int sectionIndex;
    private Parameters params;
    private int filterLevel;

    public RegexTagger() {
        filterChain = new ArrayList<IFilter>();
        events = new LinkedList<Event>();
        sectionIndex = 0;
        params = new Parameters();
        filterLevel = 0;
    }

    @Override
    public String getName () {
        return "okf_regextagger";
    }

    @Override
    public String getDisplayName () {
        return "Regex tagger";
    }

    @Override
    public void open (RawDocument input) {
        open(input, true);
    }

    @Override
    public void open (RawDocument input, boolean generateSkeleton)
    {
        srcLoc = input.getSourceLocale();
        //open with the first filter
        var topLevelFilter = filterChain.get(0);
        topLevelFilter.open(input, generateSkeleton);
        filterLevel = 0;
    }

    @Override
    public void close () {
        var topFilter = filterChain.get(0);
        topFilter.close();
    }

    @Override
    public boolean hasNext () {
        if (events.isEmpty()) {
            var topLevelFilter = filterChain.get(0);
            return topLevelFilter.hasNext();
        }

        return true; // Queue not empty
    }

    @Override
    public Event next () {
        // Cancel if requested
        if (cancelled) {
            close();
            return new Event(EventType.CANCELED);
        }

        if (events.isEmpty()) {

            // Else: build the next event(s)
            //events.add(topEvent);
        }

        Event event = events.poll();
        return event;
    }

    private String buildRefId (String parentId) {
        return "cf:" + parentId;
        //return String.format("ref-%s%s-%d", IFilter.SUB_FILTER, "parendId!!", 2022);
    }


    @Override
    public void cancel() {
        cancelled = true;
    }

    @Override
    public IParameters getParameters() {
        return params;
    }

    @Override
    public void setParameters(IParameters params) {
        this.params = (Parameters)params;
    }

    @Override
    public void setFilterConfigurationMapper(IFilterConfigurationMapper fcMapper) {
        this.fcMapper = fcMapper;
    }

    @Override
    public ISkeletonWriter createSkeletonWriter() {
        var topLevelFilter = filterChain.get(0);
        return topLevelFilter.createSkeletonWriter();
    }

    @Override
    public IFilterWriter createFilterWriter() {
        // the cascading filter writer
        return null;
    }

    @Override
    public EncoderManager getEncoderManager() {
        var topLevelFilter = filterChain.get(0);
        return topLevelFilter.getEncoderManager();
    }

    @Override
    public String getMimeType() {
        return mimeType;
    }

    @Override
    public List<FilterConfiguration> getConfigurations() {
        List<FilterConfiguration> list = new ArrayList<>();
        list.add(new FilterConfiguration("okf_cascading",
                mimeType,
                getClass().getName(),
                "Cascading filter: any documents",
                "Configuration for cascading filter",
                null,
                "*.*;"));
        return list;
    }
}
