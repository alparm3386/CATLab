package com.tm.okapi.filters.cascading;

import net.sf.okapi.common.*;
import net.sf.okapi.common.encoder.EncoderManager;
import net.sf.okapi.common.filterwriter.*;
import net.sf.okapi.common.resource.*;
import net.sf.okapi.common.skeleton.ISkeletonWriter;
import net.sf.okapi.common.filters.*;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;

public class CascadingFilterWriter implements IFilterWriter {
    private class WriterBundle {
        public WriterBundle (IFilterWriter writer, OutputStream outputStream) {
            this.writer = writer;
            this.outputStream = outputStream;
        }

        public IFilterWriter writer;
        public OutputStream outputStream;
    }

    private List<IFilter> filterChain;
    private LinkedList<WriterBundle> writerStack;
    private LinkedList<Event> eventStack;
    private LocaleId targetLocale;
    private String encoding;
    private int MARKER_SHIFT = 0x05;

    public CascadingFilterWriter(List<IFilter> filterChain) {
        this.filterChain = filterChain;
        writerStack = new LinkedList<WriterBundle>();
        //add the main filter writer
        var mainFilterWriter = filterChain.get(0).createFilterWriter();
        writerStack.push(new WriterBundle(mainFilterWriter, null)); // the output stream is handled on higher level
        //the event stack
        eventStack = new LinkedList<Event>();
    }

    @Override
    public String getName() {
        return "CascadingFilterWriter";
    }

    @Override
    public void setOptions(LocaleId locale, String defaultEncoding) {
        this.targetLocale = locale;
        this.encoding = defaultEncoding;

        //set options for the main writer
        var mainFilterWriter = writerStack.getLast().writer;
        mainFilterWriter.setOptions(locale, defaultEncoding);
    }

    @Override
    public void setOutput(String path) {
        var mainFilterWriter = writerStack.getLast().writer;
        mainFilterWriter.setOutput(path);
    }

    @Override
    public void setOutput(OutputStream output) {
        var mainFilterWriter = writerStack.getLast().writer;
        mainFilterWriter.setOutput(output);
    }

    private OutputStream createOutputStream() {
        OutputStream outputStream = new OutputStream() {
            private StringBuilder string = new StringBuilder();

            @Override
            public void write(int b) throws IOException {
                this.string.append((char) b );
            }

            //Netbeans IDE automatically overrides this toString()
            public String toString() {
                return this.string.toString();
            }
        };

        return outputStream;
    }

    private Event createStartFilterEvent() {
        StartDocument startDocument = new StartDocument("start_doc: custom");
        startDocument.setEncoding("UTF-8", false);
        startDocument.setLocale(targetLocale);
        //startDocument.setMimeType(getMimeType());
        startDocument.setLineBreak("\r\n");
        //startDocument.setFilterId(getName());
        //startDocument.setFilterParameters(getParameters());
        //startDocument.setFilterWriter(getFilterWriter());
        //startDocument.setName(getDocumentName());
        //startDocument.setMultilingual(isMultilingual());
        //LOGGER.debug("Start Document for {}", startDocument.getId()); //$NON-NLS-1$
        return new Event(EventType.START_DOCUMENT, startDocument);
    }

    private Event createEndFilterEvent() {
        var endDocument = new Ending("end_doc: custom");
        //LOGGER.debug("End Document for {}", endDocument.getId()); //$NON-NLS-1$
        return new Event(EventType.END_DOCUMENT, endDocument);
    }

    @Override
    public Event handleEvent(Event event) {
        if (event.getEventType() == EventType.CUSTOM) {
            //create filter writer for the sub-filter and add to the stack
            var filterLevel = writerStack.size();
            var writer = filterChain.get(filterLevel).createFilterWriter();
            var enccoding = writer.getEncoderManager().getEncoding();
            //the output stream
            var outputStream = new ByteArrayOutputStream();
            writer.setOutput(outputStream);
            writer.setOptions(targetLocale, encoding);
            writerStack.push(new WriterBundle(writer, outputStream));
            //cache the event
            eventStack.push(event);

            return event;
        } else if (event.getEventType() == EventType.START_SUBFILTER) {
            var currentWriter = writerStack.peek().writer;
            currentWriter.handleEvent(createStartFilterEvent()); //it forces creating writer
            var tmpEvent = currentWriter.handleEvent(event);
            return tmpEvent;
        } else if (event.getEventType() == EventType.END_SUBFILTER) {
            //handle the event with the sub-writer
            var writerBundle = writerStack.pop();
            var currentWriter = writerBundle.writer;
            currentWriter.handleEvent(event);
            currentWriter.handleEvent(createEndFilterEvent());
            currentWriter.close();

            //merge back the text content
            var bytes = ((ByteArrayOutputStream)writerBundle.outputStream).toByteArray();
            var textContent = new String(bytes, StandardCharsets.UTF_8);
            var customEvent = eventStack.pop();
            var parentEvent = ((EventContainer)customEvent.getResource()).getEvent();
            var tu = (TextUnit)parentEvent.getResource();
            //shift back the current level markers
            var filterLevel = writerStack.size() - 1;
            textContent = unshiftMarkers(textContent, filterLevel);
            var tf = new TextFragment(textContent, tu.getSource().getFirstContent().getCodes());
            tu.setTarget(targetLocale, new TextContainer(tf));
            writerBundle = writerStack.peek();
            return writerBundle.writer.handleEvent(parentEvent);
        } else if (event.getEventType() == EventType.TEXT_UNIT) {
            //shift the upper level codes so that they won't rendered by this filter
            var filterLevel = writerStack.size() - 1;
            var tu = (TextUnit)event.getResource();
            //we don't need the segmentation anymore
            tu.removeAllSegmentations();
            var tf = tu.getTarget(targetLocale).getFirstContent();
            shiftMarkers(tf, filterLevel); //shift the upper level markers so that won't be rendered with this filter

            var currentWriter = writerStack.peek().writer;
            var tmpEvent = currentWriter.handleEvent(event);
            return tmpEvent;
        } else {
            var currentWriter = writerStack.peek().writer;
            return currentWriter.handleEvent(event);
        }
    }

    public void shiftMarkers(TextFragment tf, int filterLevel) {
        var codedText = tf.getCodedText();
        StringBuilder sbCodedText = new StringBuilder(codedText);
        var codeCntr = 0;
        var newCodes = new ArrayList<Code>();
        var idsToRemove = new ArrayList<Integer>();
        for (int i = 0; i < codedText.length(); i++) {
            final char chr = codedText.charAt(i);
            if (TextFragment.isMarker(chr)) {
                int idx = TextFragment.toIndex(codedText.charAt(i + 1));
                var code = tf.getCode(idx);
                var flevel = code.getProperty("flevel");
                if (flevel != null && Integer.parseInt(flevel.getValue()) < filterLevel) {
                    //shift the marker
                    sbCodedText.setCharAt(i, (char)(chr + MARKER_SHIFT));
                } else {
                    idsToRemove.add(TextFragment.toIndex(codedText.charAt(i + 1)));
                    sbCodedText.setCharAt(i + 1, TextFragment.toChar(codeCntr));
                    newCodes.add(code);
                    codeCntr++;
                }
            }
        }

        //reset the ids for the parent tu
        for (int id : idsToRemove) {
            for (int i = 0; i < sbCodedText.length(); i++) {
                final char chr = sbCodedText.charAt(i);
                if (TextFragment.isMarker((char)(chr - MARKER_SHIFT))) {
                    if (sbCodedText.charAt(i + 1) - TextFragment.CHARBASE > id)
                        sbCodedText.setCharAt(i + 1,(char)(sbCodedText.charAt(i + 1) - 1));
                }
            }
        }

        //reset the text fragment
        tf.setCodedText(sbCodedText.toString(), newCodes, true);
    }

    public String unshiftMarkers(String codedText, int filterLevel) {
        StringBuilder sbCodedText = new StringBuilder(codedText);
        for (int i = 0; i < sbCodedText.length(); i++) {
            final char chr = codedText.charAt(i);
            if (TextFragment.isMarker((char)(chr - MARKER_SHIFT))) {
                sbCodedText.setCharAt(i, (char)(chr - MARKER_SHIFT));
            }
        }

        return sbCodedText.toString();
    }

    @Override
    public void close() {
        var mainFilterWriter = writerStack.getLast().writer;
        mainFilterWriter.close();
    }

    @Override
    public IParameters getParameters() {
        return null;
    }

    @Override
    public void setParameters(IParameters params) {
    }

    @Override
    public void cancel() {
        var mainFilterWriter = writerStack.getLast().writer;
        mainFilterWriter.cancel();
    }

    @Override
    public EncoderManager getEncoderManager() {
        var mainFilterWriter = writerStack.getLast().writer;
        return mainFilterWriter.getEncoderManager();
    }

    @Override
    public ISkeletonWriter getSkeletonWriter() {
        var mainFilterWriter = writerStack.getLast().writer;
        return mainFilterWriter.getSkeletonWriter();
    }
}
