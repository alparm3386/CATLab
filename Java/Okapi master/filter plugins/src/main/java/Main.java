import com.tm.okapi.filters.cascading.CascadingFilter;
import com.tm.okapi.filters.cascading.Editor;
import com.tm.okapi.filters.cascading.XliffViewer;
import com.tm.okapi.steps.XliffTranslationStep;
import net.sf.okapi.common.*;
import net.sf.okapi.common.exceptions.OkapiException;
import net.sf.okapi.common.filters.*;
import net.sf.okapi.common.filterwriter.IFilterWriter;
import net.sf.okapi.common.filterwriter.XLIFFWriter;
import net.sf.okapi.common.filterwriter.XLIFFWriterParameters;
import net.sf.okapi.common.pipelinedriver.PipelineDriver;
import net.sf.okapi.common.resource.*;
import net.sf.okapi.filters.openxml.OpenXMLFilter;
import net.sf.okapi.filters.plaintext.PlainTextFilter;
import net.sf.okapi.filters.regex.RegexFilter;
import net.sf.okapi.filters.xliff.Parameters;
import net.sf.okapi.filters.xml.XMLFilter;
import net.sf.okapi.lib.segmentation.SRXDocument;
import net.sf.okapi.steps.common.FilterEventsWriterStep;
import net.sf.okapi.steps.common.RawDocumentToFilterEventsStep;
import net.sf.okapi.steps.segmentation.SegmentationStep;
import org.eclipse.swt.SWT;
import org.eclipse.swt.widgets.Display;
import org.eclipse.swt.widgets.Shell;

import java.io.*;
import java.lang.reflect.Method;
import java.net.URL;
import java.net.URLClassLoader;
import java.nio.file.*;
import java.util.*;
import java.util.jar.JarEntry;
import java.util.jar.JarInputStream;

public class Main {
    public static void main(String[] originalArgs) {
        System.out.println("Start...");
        filterLoaderTest();
        //simpleFilterTest();
        //customFilterTest();
        //cascadingFilterTest();
        //cascadingFilterPipelineTest();
        //testSegmenter();
        //tuTest();
        //uiTest();
        //var main = new Main();
        //main.classLoaderTest();
        //xliffViewerTest();
    }

    public static void filterLoaderTest()  {
        try {
            URL[] tmpUrls = new URL[1];
            URL url = new File("C:/Development/Website/CAT/Okapi%20master/okapi/applications/rainbow/target/dropins/filters.jar").toURI().toURL();
            tmpUrls[0] = url;
            URLClassLoader loader = URLClassLoader.newInstance(tmpUrls, Thread.currentThread().getContextClassLoader());
            Class<?> cls = Class.forName("com.tm.okapi.filters.cascading.CascadingFilter", false, loader);

            System.out.println(cls.toString());
            System.in.read();
            int a = 0;
        } catch (Exception ex) {

        }
    }

    public static void xliffViewerTest() {
        String sPath = "C:\\Development\\OKAPI\\Test files\\xls_html_simple.xlsx.xliff";
        XliffViewer xliffViewer = new XliffViewer(new Shell(), SWT.CLOSE | SWT.TITLE | SWT.APPLICATION_MODAL);
        xliffViewer.open(sPath, true);
    }

    private void inspectFile (File file) {
        try {
            // Make sure there is something to discover
            if (( file == null ) || !file.exists() ) return;

            // Create a temporary class loader
            URL[] tmpUrls = new URL[1];
            URL url = file.toURI().toURL();
            tmpUrls[0] = url;
            URLClassLoader loader = URLClassLoader.newInstance(tmpUrls, this.getClass().getClassLoader());

            // Introspect the classes
            FileInputStream fis = new FileInputStream(file);
            JarInputStream jarFile = new JarInputStream(fis);
            JarEntry entry;
            Class<?> cls = null;
            while ( true ) {
                cls = null; // Try to help unlocking the file
                if ( (entry = jarFile.getNextJarEntry()) == null ) break;
                String name = entry.getName();
                if ( name.endsWith(".class") ) {
                    name = name.substring(0, name.length()-6).replace('/', '.');
                    try {
                        cls = Class.forName(name, false, loader);
                        // Skip interfaces
                        if ( cls.isInterface() ) continue;
                    }
                    catch ( Throwable e ) {
                        // If the class cannot be create for some reason, we skip it silently
                    }
                    cls = null; // Try to help unlocking the file
                }
            }
            if ( jarFile != null ) {
                jarFile.close();
                jarFile = null; // Try to help unlocking the file
                fis.close();
                fis = null; // Try to help unlocking the file
                file = null; // Try to help unlocking the file
            }
            cls = null; // Try to help unlocking the file
            loader = null; // Try to help unlocking the file
        }
        catch ( IOException e ) {
            throw new OkapiException("IO error when inspecting a file for plugins.", e);
        }
    }
    public void classLoaderTest() {
        try {
            // Run a java app in a separate system process
            //Process proc = Runtime.getRuntime().exec("java -jar C:/Development/OKAPI/dropins/Ocelot-3.0.jar");
            // Then retreive the process output
            //InputStream in = proc.getInputStream();
            //InputStream err = proc.getErrorStream();
            //if (true)
            //    return;

            String currDir = System.getProperty("user.dir");
            File ocelotJar = new File("C:/Development/OKAPI/dropins/Ocelot-3.0.jar");
            URLClassLoader child = new URLClassLoader(
                    new URL[]{ocelotJar.toURI().toURL()},
                    this.getClass().getClassLoader()
            );
            Class classToLoad = Class.forName("com.vistatec.ocelot.OcelotStarter", true, child);
            //Class classToLoad = Class.forName("com.tm.okapi.filters.cascading.CascadingFilter", true, child);
            Method startMethod = classToLoad.getDeclaredMethod("start");
            Method openFileMethod = classToLoad.getDeclaredMethod("openFile", String.class);
            Object instance = classToLoad.newInstance();
            startMethod.invoke(instance);
            String sPath = "C:\\Development\\OKAPI\\Test files\\xls_html_simple.xlsx.xliff";
            openFileMethod.invoke(instance, sPath);
            Thread.sleep(400000);

            int b = 0;
        } catch (Exception ex) {
            int a = 0;
        }
    }

    public static void uiTest() {
        try {
            Display d = new Display();
            Shell shell = new Shell(d);
            shell.setSize(500,500);
            shell.setText("A Shell Example");
            shell.open();

            net.sf.okapi.filters.xliff.ui.Editor editor = new net.sf.okapi.filters.xliff.ui.Editor();
            Parameters params = new net.sf.okapi.filters.xliff.Parameters();
            IContext context = new BaseContext();
            //editor.edit(params, false, context);

            ClassLoader classLoader = Main.class.getClassLoader();
            InputStream inputStream = classLoader.getResourceAsStream("segmentation/All languages segmentation.srx");
            byte[] bytes = inputStream.readAllBytes();

            Editor editorCascading = new Editor();
            com.tm.okapi.filters.cascading.Parameters parameters = new com.tm.okapi.filters.cascading.Parameters();
            File file = new File("C:\\Development\\OKAPI\\custom filters\\okf_cascading@xls_html_regex.fprm");
            parameters.fromString(parameters.toString());
            parameters.load(file.toURI().toURL(), false);
            if (editorCascading.edit(parameters, false, context)) {
                parameters.save("C:\\Alpar\\cascading filter.fprm");
            }
        } catch (Exception ex) {
            System.out.println(ex.toString());
        }
    }

    public static void cascadingFilterPipelineTest()
            throws OkapiException {
        String sTempOutPath = null;
        try {
            String filePath = "C:/Development/Okapi/Test files/xls_html_simple.xlsx";
            File file = new File(filePath);
            byte[] fileContent = Files.readAllBytes(Path.of(filePath));
            String SRX_FILE = "C:/Development/Okapi/All languages segmentation.srx";
            IFilter filter = new CascadingFilter();
            IParameters param = filter.getParameters();
            param.load(new FileInputStream("C:/Development/Okapi/Custom filters/okf_cascading@xls_html_regex.fprm"), false);
            filter.setParameters(param);

            String TEMP_DIR = "C:\\Alpar";

            long lStart = System.currentTimeMillis();

            // the raw document
            ByteArrayInputStream fis = new ByteArrayInputStream(fileContent);
            RawDocument rawDoc = new RawDocument(fis, "UTF-8", LocaleId.fromString("en"));
            rawDoc.setTargetLocale(LocaleId.fromString("fr"));

            // the segmentation
            SegmentationStep segStep = new SegmentationStep();
            net.sf.okapi.steps.segmentation.Parameters segParams = (net.sf.okapi.steps.segmentation.Parameters) segStep.getParameters();
            segParams.setSegmentSource(true);
            segParams.setSegmentTarget(true);
            segParams.setSourceSrxPath(SRX_FILE);
            segParams.setTargetSrxPath(SRX_FILE);
            // segParams.setCopySource(true);
            segParams.setSegmentTarget(true);

            // the pipeline
            PipelineDriver driver = new PipelineDriver();
            // the raw document to filter events step
            RawDocumentToFilterEventsStep rawDocToFilterEventStep = new RawDocumentToFilterEventsStep(filter);
            driver.addStep(rawDocToFilterEventStep);

            // add segmentation step
            driver.addStep(segStep);

            // add the translate step
            XliffTranslationStep translateStep = new XliffTranslationStep(null);
            translateStep.setSourceLocale(LocaleId.fromString("en"));
            translateStep.setTargetLocale(LocaleId.fromString("fr"));
            driver.addStep(translateStep);

            sTempOutPath = Paths.get(TEMP_DIR, UUID.randomUUID().toString()).toString();

            // Add the filter writer step
            FilterEventsWriterStep fewStep = new FilterEventsWriterStep();
            IFilterWriter fw = filter.createFilterWriter();
            fw.setOutput(sTempOutPath);
            fewStep.setFilterWriter(fw);
            driver.addStep(fewStep);

            // Create the raw document and set the output
            driver.addBatchItem(rawDoc, new java.io.File(sTempOutPath).toURI(), "UTF-8");
            // Process
            driver.processBatch();

            long lElapsed = System.currentTimeMillis() - lStart;
            System.out.println("CreateDoc: " + filePath + " -> " + lElapsed + "ms.");

            byte[] aRet = Files.readAllBytes(Path.of(sTempOutPath));

        } catch (Exception ex) {
            throw new OkapiException();
            // return null;
        } finally {
            // cleanup
            if (sTempOutPath != null)
                try {
                    new File(sTempOutPath).delete();
                } catch (Exception ex) {
                }
        }

    }


    public static void cascadingFilterTest() {
        try {
            RegexFilter regex  = new net.sf.okapi.filters.regex.RegexFilter();
            // Create a filter object
            IFilter filter = new CascadingFilter();
            IParameters param = filter.getParameters();
            //param.load(new FileInputStream("C:/Development/Okapi/Custom filters/okf_cascading@xml_html_regex.fprm"), false);
            param.load(new FileInputStream("C:/Development/Okapi/Custom filters/okf_cascading@xls_html_regex.fprm"), false);
            //param.load(new FileInputStream("C:/Development/Okapi/Custom filters/okf_cascading@primark_articles.fprm"), false);
            filter.setParameters(param);

            // Creates the RawDocument object
            //String filePath = "C:/Development/Okapi/Test files/xml_simple.xml";
            //String filePath = "C:/Development/Okapi/Test files/xml_primark_small.xml";
            //String filePath = "C:/Development/Okapi/Test files/xml_simple_small.xml";
            //String filePath = "C:/Development/Okapi/Test files/Janet Yellen_small.docx";
            String filePath = "C:/Development/Okapi/Test files/xls_html_simple.xlsx";
            //String filePath = "C:/Development/Okapi/Test files/Primark article 560945.xml";

            File file = new File(filePath);
            byte[] aFileContent = Files.readAllBytes(Path.of(filePath));
            ByteArrayInputStream fis = new ByteArrayInputStream(aFileContent);
            RawDocument rawDoc = new RawDocument(fis, "UTF-8", LocaleId.fromString("en"));
            // Opens the document
            filter.open(rawDoc);

            //the segmenter
            SRXDocument srxDoc = new SRXDocument();
            srxDoc.loadRules("C:/Development/Okapi/All languages segmentation.srx");
            ISegmenter segmenter = srxDoc.compileLanguageRules(LocaleId.fromString("en"), null);

            //xliff writer
            XLIFFWriter xliffWriter = new XLIFFWriter();
            xliffWriter.setOutput("C:\\Alpar\\out.xlf");
            xliffWriter.setOptions(LocaleId.fromString("fr"), "utf-8");
            XLIFFWriterParameters xlfParams = xliffWriter.getParameters();
            xlfParams.setPlaceholderMode(false);
            xliffWriter.setParameters(xlfParams);

            //the filter writer
            IFilterWriter writer = filter.createFilterWriter();
            writer.setOptions(LocaleId.fromString("fr"), "utf-8");
            // Set the output
            writer.setOutput("C:\\Alpar\\xmlTest_fr.xml");
            // Get the events from the input document
            while ( filter.hasNext() ) {
                Event event = filter.next();
                // do something with the event...
                // Here, if the event is TEXT_UNIT, we display the key and the extracted text
                if (event.isTextUnit()) {
                    TextUnit tu = (TextUnit)event.getResource();
                    System.out.println("--");
                    System.out.println("key=["+tu.getName()+"]");
                    //System.out.println("text=["+tu.getSource()+"]");
                    System.out.println("text=["+tu.getSource().getCodedText()+"]");

                    //do the segmentation
                    TextContainer tcSource = tu.getSource();
                    boolean bSegmented = tcSource.hasBeenSegmented();
                    TextFragment tfSource = tcSource.getFirstContent();
                    //tfSource.balanceMarkers();
                    //tfSource.cleanCodes();
                    tfSource.setCodedText(tfSource.getCodedText(), true);
                    //int segments = segmenter.computeSegments(tcSource);
                    //tcSource.getSegments().create(segmenter.getRanges());
                    //var segs = tcSource.getSegments();
                    bSegmented = tcSource.hasBeenSegmented();

                    //do modifications
                    changeTU((TextUnit)event.getResource());
                }
                writer.handleEvent(event);
                xliffWriter.handleEvent(event);
            }
            // Close the input document
            filter.close();
            writer.close();
            xliffWriter.close();
        } catch (Exception ex) {
            System.out.println(ex.toString());
        }
    }

    public static void changeTU (TextUnit tu) {
        //if (true)
        //    return;
        // Check if this unit can be modified
        if ( !tu.isTranslatable() ) return; // If not, return without changes
        TextContainer tc = tu.createTarget(LocaleId.fromString("fr"), false, IResource.COPY_ALL);
        ISegments segs = tc.getSegments();
        for ( Segment seg : segs ) {
            TextFragment tf = seg.getContent();
            tf.setCodedText(tf.getCodedText().toUpperCase());
            //tf.setCodedText("Il s'agit d'un contenu HTML intégré dans une balise p.");
        }
    }

    public static void simpleFilterTest() {
        try {
            // Create a filter object
            //IFilter filter = new OpenXMLFilter();
            //IFilter filter = new XmlStreamFilter();
            IFilter filter = new XMLFilter();
            //IFilter filter = new HtmlFilter();

            // Creates the RawDocument object
            //String filePath = "C:/Development/Okapi/Test files/Janet Yellen_small.docx";
            String filePath = "C:/Development/Okapi/Test files/xml_simple.xml";
            //String filePath = "C:/Development/Okapi/Test files/xls_html_simple.xlsx";
            File file = new File(filePath);
            byte[] aFileContent = Files.readAllBytes(Path.of(filePath));
            ByteArrayInputStream fis = new ByteArrayInputStream(aFileContent);
            RawDocument rawDoc = new RawDocument(fis, "UTF-8", LocaleId.fromString("en"));
            // Opens the document
            filter.open(rawDoc);

            //the filter writer
            IFilterWriter writer = filter.createFilterWriter();
            writer.setOptions(LocaleId.fromString("fr"), "utf-8");
            // Set the output
            writer.setOutput("C:\\Alpar\\xmlTest_fr.xml");

            //xliff writer
            XLIFFWriter xliffWriter = new XLIFFWriter();
            xliffWriter.setOutput("C:\\Alpar\\out.xlf");
            xliffWriter.setOptions(LocaleId.fromString("fr"), "utf-8");
            XLIFFWriterParameters xlfParams = xliffWriter.getParameters();
            xlfParams.setPlaceholderMode(false);
            xliffWriter.setParameters(xlfParams);

            // Get the events from the input document
            while ( filter.hasNext() ) {
                Event event = filter.next();
                // do something with the event...
                // Here, if the event is TEXT_UNIT, we display the key and the extracted text
                if ( event.getEventType() == EventType.TEXT_UNIT ) {
                    TextUnit tu = (TextUnit)event.getResource();
                    System.out.println("--");
                    System.out.println("key=["+tu.getName()+"]");
                    System.out.println("text=["+tu.getSource()+"]");

                    //do modifications
                    changeTU((TextUnit)event.getResource());
                }
                writer.handleEvent(event);
                xliffWriter.handleEvent(event);
            }
            // Close the input document
            filter.close();
            writer.close();
            xliffWriter.close();
        } catch (Exception ex) {
            System.out.println(ex.toString());
        }
    }

    public static void customFilterTest() {
        try {
            // Create a filter object
            IFilter filter = new OpenXMLFilter();
            //Filter filter = new XmlStreamFilter();
            String filterPath = "C:/Development/Okapi/custom filters/okf_openxml@xls_html.fprm";
            //String filterPath = "C:/Development/Okapi/custom filters/okf_xmlstream@xml_html_simple.fprm";
            if (filterPath != null) {
                //filter config mapper
                IFilterConfigurationMapper fcMapper = new FilterConfigurationMapper();
                DefaultFilters.setMappings(fcMapper, false, true);
                filter.setFilterConfigurationMapper(fcMapper);

                //the custom filter
                FileInputStream fprmInputStream = new FileInputStream(filterPath);
                IParameters parameters = filter.getParameters();
                parameters.load(fprmInputStream, true);
                filter.setParameters(parameters);
            }

            //xliff writer
            XLIFFWriter xliffWriter = new XLIFFWriter();
            xliffWriter.setOutput("C:\\Alpar\\out.xlf");
            xliffWriter.setOptions(LocaleId.fromString("fr"), "utf-8");
            XLIFFWriterParameters xlfParams = xliffWriter.getParameters();
            xlfParams.setPlaceholderMode(false);
            xliffWriter.setParameters(xlfParams);

            //the filter writer
            IFilterWriter writer = filter.createFilterWriter();
            writer.setOptions(LocaleId.fromString("fr"), "utf-8");
            // Set the output
            writer.setOutput("C:\\Alpar\\xmlTest_fr.xml");

            // Creates the RawDocument object
            String filePath = "C:/Development/Okapi/Test files/xls_html_simple.xlsx";
            //String filePath = "C:/Development/Okapi/Test files/xmlTest.xml";
            File file = new File(filePath);
            byte[] aFileContent = Files.readAllBytes(Path.of(filePath));
            ByteArrayInputStream fis = new ByteArrayInputStream(aFileContent);
            RawDocument rawDoc = new RawDocument(fis, "UTF-8", LocaleId.fromString("en"));
            // Opens the document
            filter.open(rawDoc);
            // Get the events from the input document
            while ( filter.hasNext() ) {
                Event event = filter.next();
                // do something with the event...
                // Here, if the event is TEXT_UNIT, we display the key and the extracted text
                if (event.getEventType() == EventType.TEXT_UNIT) {
                    TextUnit tu = (TextUnit)event.getResource();
                    System.out.println("--");
                    System.out.println("key=["+tu.getName()+"]");
                    System.out.println("text=["+tu.getSource()+"]");
                    System.out.println("text=["+tu.getSource().getCodedText()+"]");
                }
                writer.handleEvent(event);
                xliffWriter.handleEvent(event);
            }
            // Close the input document
            filter.close();
            writer.close();
            xliffWriter.close();
        } catch (Exception ex) {
            System.out.println(ex.toString());
        }
    }

    public static void testSegmenter() {
        try {
            // Create and load the SRX document
            SRXDocument doc = new SRXDocument();
            File f = new File("C:/Development/Okapi/All languages segmentation.srx");
            doc.loadRules(f.getAbsolutePath());

            // Obtain a segmenter for English
            ISegmenter segmenter = doc.compileLanguageRules(LocaleId.fromString("en"), null);

            // Plain text case
            int count = segmenter.computeSegments("Part 1. Part 2.");
            System.out.println("count="+ count);
            for ( Range range : segmenter.getRanges() ) {
                System.out.println(String.format("start=%d, end=%d",
                        range.start, range.end));
            }

            // TextContainer case
            TextFragment tf = new TextFragment();
            tf.append(TextFragment.TagType.OPENING, "span", "<span>");
            tf.append("Part 1.");
            tf.append(TextFragment.TagType.CLOSING, "span", "</span>");
            tf.append(" Part 2.");
            tf.append(TextFragment.TagType.PLACEHOLDER, "alone", "<alone/>");
            tf.append(" Part 3.");
            TextContainer tc = new TextContainer(tf);
            segmenter.computeSegments(tc);
            ISegments segs = tc.getSegments();
            segs.create(segmenter.getRanges());
            ISegments segs2 = tc.getSegments();
            for ( Segment seg : segs ) {
                System.out.println("segment=[" + seg.toString() + "]");
            }
        }
        catch ( Throwable e ) {
            e.printStackTrace();
        }
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

    public static void tuTest() {
        TextUnit tmpTu = new TextUnit("id1");
        TextFragment tmpTf = tmpTu.setSourceContent(new TextFragment("Text in "));
        tmpTf.append(TextFragment.TagType.OPENING, "bold", "<b>");
        tmpTf.append("bold");
        tmpTf.append(TextFragment.TagType.CLOSING, "bold", "</b>");
        tmpTf.append(".");
        System.out.println(tmpTf.toString());


        IFilter filter = new PlainTextFilter();
        filter.open(new RawDocument(tmpTf.getCodedText(), new LocaleId("en")));
        while ( filter.hasNext() ) {
            Event event = filter.next();
            // do something with the event...
            // Here, if the event is TEXT_UNIT, we display the key and the extracted text
            if (event.isTextUnit()) {
                TextUnit tu = (TextUnit)event.getResource();
                TextFragment tf = tu.getSource().getFirstContent();
                System.out.println(tu);
            }
        }

    }
}
