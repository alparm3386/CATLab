package com.tm.okapi.filters.cascading;

import net.sf.okapi.common.*;
import net.sf.okapi.common.encoder.EncoderManager;
import net.sf.okapi.common.exceptions.OkapiException;
import net.sf.okapi.common.exceptions.OkapiIOException;
import net.sf.okapi.common.filters.*;
import net.sf.okapi.common.filterwriter.*;
import net.sf.okapi.common.resource.*;
import net.sf.okapi.common.skeleton.*;
import org.apache.commons.io.FileUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;

@UsingParameters(Parameters.class)
public class CascadingFilter implements IFilter {

    private LocaleId srcLoc;
    private boolean cancelled = false;
    private LinkedList<Event> events;
    private IFilterConfigurationMapper fcMapper;
    private String mimeType = "application/octet-stream";
    private final Logger logger = LoggerFactory.getLogger(getClass());
    private int sectionIndex;
    private Parameters params;
    private int filterLevel;
    private int MARKER_SHIFT = 0x05;

    public CascadingFilter() {
        events = new LinkedList<Event>();
        sectionIndex = 0;
        params = new Parameters();
        filterLevel = 0;
    }

    @Override
    public String getName () {
        return "okf_cascading";
    }

    @Override
    public String getDisplayName () {
        return "Cascading filter";
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
        var topLevelFilter = params.getFilterChain().get(0);
        topLevelFilter.open(input, generateSkeleton);
        filterLevel = 0;
    }

    @Override
    public void close () {
        var topLevelFilter = params.getFilterChain().get(0);
        topLevelFilter.close();
    }

    @Override
    public boolean hasNext () {
        if (events.isEmpty()) {
            var topLevelFilter = params.getFilterChain().get(0);
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
            var filterChain = params.getFilterChain();
            var topLevelFilter = filterChain.get(0);
            var topEvent = topLevelFilter.next();

            // Else: build the next event(s)
            if ( topEvent.isTextUnit() ) {
                if (filterChain.size() > 1) {
                    var subFilter = filterChain.get(1);
                    processWithSubfilter(topEvent, subFilter);
                } else
                    events.add(topEvent);

            } else {
                if (topEvent.isStartDocument()) {
                    //reset the filter parameters in teh start document resource
                    var startDoc = (StartDocument)topEvent.getResource();
                    startDoc.setFilterParameters(getParameters());
                }
                events.add(topEvent);
            }
        }

        Event event = events.poll();
        return event;
    }

    private String buildRefId (String parentId) {
        return "cf:" + parentId;
        //return String.format("ref-%s%s-%d", IFilter.SUB_FILTER, "parendId!!", 2022);
    }

    private void processWithSubfilter (Event parentEvent, IFilter filterToUse)
    {
        filterLevel++;
        //logger.info("sf: {}", dataToUse);
        if (parentEvent.getEventType() != EventType.TEXT_UNIT)
            throw new OkapiIOException("Invalid event");

        //get the text unit
        ITextUnit tuParent = (TextUnit)parentEvent.getResource();
        //System.out.println("top filter: text=["+tuParent.getSource().getCodedText()+"]");
        //set the level for the codes
        var codes = tuParent.getSource().getFirstContent().getCodes(); //we have only one text fragment at this point.
        for (var code : codes ) {
            code.setProperty(new Property("flevel", String.valueOf(filterLevel - 1)));
        }

        //carry the original event in a custom event
        var parentSkeleton = tuParent.getSkeleton();
        var eventContainer = new EventContainer(buildRefId(tuParent.getId()), parentEvent);
        var customEvent = new Event(EventType.CUSTOM, eventContainer);
        events.add(customEvent);

        // Create the sub-filter wrapper
        SubFilter subfilter = new SubFilter(filterToUse, filterToUse.getEncoderManager().getEncoder(),
                ++sectionIndex, tuParent.getId(), tuParent.getName());

        //shift the markers so that it won't be processed
        var tfParent = tuParent.getSource().createJoinedContent();
        var codedText = shiftMarkers(tfParent);
        subfilter.open(new RawDocument(codedText, srcLoc));
        while (subfilter.hasNext()) {
            // Get the event
            Event event = subfilter.next();
            // Extra parsing to fix Markdown filter misses
            if (event.isTextUnit()) {
                var filterChain = params.getFilterChain();
                var tu = event.getTextUnit();

                if (filterChain.size() > filterLevel + 1) {
                    var subFilter = filterChain.get(filterLevel + 1);
                    var tfSubfiltered = tu.getSource().getFirstContent();

                    unshiftMarkers(tfParent, tfSubfiltered);
                    tu.setSourceContent(tfSubfiltered);
                    processWithSubfilter(event, subFilter);
                } else {
                    var tfSubfiltered = tu.getSource().getFirstContent();
                    unshiftMarkers(tfParent, tfSubfiltered);
                    tu.setSourceContent(tfSubfiltered);

                    events.add(event);
                }
            } else {
                events.add(event);
            }
        }
        subfilter.close();
        filterLevel--;
    }

    private static char tagTypeToMarker(TextFragment.TagType tagtype) {
        if (tagtype == TextFragment.TagType.OPENING)
            return (char)TextFragment.MARKER_OPENING;
        else if (tagtype == TextFragment.TagType.CLOSING)
            return (char)TextFragment.MARKER_CLOSING;
        else if (tagtype == TextFragment.TagType.PLACEHOLDER)
            return (char)TextFragment.MARKER_ISOLATED;

        throw new OkapiException();
    }

    private String shiftMarkers(TextFragment tf) {
        var codedText = tf.getCodedText();
        StringBuilder sbCodedText = new StringBuilder(codedText);
        for (int i = 0; i < sbCodedText.length(); i++) {
            final char chr = codedText.charAt(i);
            if (TextFragment.isMarker(chr)) {
                //shift the marker
                sbCodedText.setCharAt(i, (char)(chr + MARKER_SHIFT));
            }
        }

        //reset the coded text
        return sbCodedText.toString();
    }

    public void unshiftMarkers(TextFragment tfParent, TextFragment tfSubfiltered) {
        //renumber the code ids in the subfiltered text fragment
        var codedText = tfSubfiltered.getCodedText();
        var sbCodedText = new StringBuilder(codedText);
        var lstNewCodes = new ArrayList<Code>();
        int idx = 0;
        for (int i = 0; i < sbCodedText.length(); i++) {
            final char chr = codedText.charAt(i);
            //new markers, shift the idx with the num of the old codes
            if (TextFragment.isMarker(chr)) {
                int oldIdx = TextFragment.toIndex(codedText.charAt(i + 1));
                sbCodedText.setCharAt(i + 1, TextFragment.toChar(idx));
                lstNewCodes.add(tfSubfiltered.getCode(oldIdx));
                idx++;
            }
            //unshift the markers for the old codes
            if (TextFragment.isMarker((char)(chr - MARKER_SHIFT))) {
                int oldIdx = TextFragment.toIndex(codedText.charAt(i + 1));
                sbCodedText.setCharAt(i, (char)(chr - MARKER_SHIFT));
                sbCodedText.setCharAt(i + 1, TextFragment.toChar(idx));
                lstNewCodes.add(tfParent.getCode(oldIdx));
                idx++;
            }
        }

        //reset the text fragment
        tfSubfiltered.setCodedText(sbCodedText.toString(), lstNewCodes);
        tfSubfiltered.renumberCodes(1);
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
        var filterChain = params.getFilterChain();
        var topLevelFilter = filterChain.get(0);
        return topLevelFilter.createSkeletonWriter();
    }

    @Override
    public IFilterWriter createFilterWriter() {
        // the cascading filter writer
        var filterChain = params.getFilterChain();
        var filterWriter = new CascadingFilterWriter(filterChain);
        return filterWriter;
    }

    @Override
    public EncoderManager getEncoderManager() {
        var filterChain = params.getFilterChain();
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
