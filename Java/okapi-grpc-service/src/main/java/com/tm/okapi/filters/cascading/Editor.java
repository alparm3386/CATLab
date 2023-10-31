package com.tm.okapi.filters.cascading;

import net.sf.okapi.common.*;
import net.sf.okapi.common.filters.*;
import net.sf.okapi.common.filterwriter.IFilterWriter;
import net.sf.okapi.common.filterwriter.XLIFFWriter;
import net.sf.okapi.common.resource.RawDocument;
import net.sf.okapi.common.resource.TextUnit;
import net.sf.okapi.common.ui.Dialogs;
import net.sf.okapi.common.ui.InputDialog;
import net.sf.okapi.common.ui.filters.FilterConfigurationsDialog;
import net.sf.okapi.common.ui.genericeditor.GenericEditor;
import net.sf.okapi.common.uidescription.IEditorDescriptionProvider;
import net.sf.okapi.lib.segmentation.SRXDocument;
import org.eclipse.swt.custom.BusyIndicator;
import org.eclipse.swt.custom.CLabel;
import org.eclipse.swt.widgets.*;
import org.eclipse.swt.SWT;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;

import java.io.ByteArrayInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.lang.reflect.Method;
import java.net.URL;
import java.net.URLClassLoader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.regex.Pattern;

import org.eclipse.swt.events.MouseAdapter;
import org.eclipse.swt.events.MouseEvent;
import org.eclipse.swt.widgets.Event;

@EditorFor(Parameters.class)
public class Editor implements IParametersEditor {
	//protected Shell shlFilterParameters;// = new Shell((Shell)null, SWT.CLOSE | SWT.TITLE | SWT.APPLICATION_MODAL);
	private Shell shlFilterParameters;
	private static FilterConfigurationMapper fcMapper;
	private java.util.List<FilterConfiguration> filterConfigChain;
	private List swtListFilters;
	private Parameters parameters;
	private boolean result = false;
	private CLabel lblTestFilePath;

	static {
		//load the filters
		fcMapper = new FilterConfigurationMapper();
		DefaultFilters.setMappings(fcMapper, true, true);
	}

	public Editor() {
		//the filter chain
		filterConfigChain = new ArrayList<FilterConfiguration>();
	}

	//we need it only for the window builder
	//public Object open() {
	//	edit(null, true, null);
	//	return null;
	//}

	private boolean showConfirmDialog(String title, String message) {
		MessageBox messageBox = new MessageBox(shlFilterParameters, SWT.ICON_QUESTION
				| SWT.YES | SWT.NO);
		messageBox.setMessage(message);
		messageBox.setText(title);
		int response = messageBox.open();
		if (response == SWT.YES)
			return true;
		else
			return false;
	}

	private void showMessageBox(String title, String message) {
		var shell = new Shell();
		MessageBox messageBox = new MessageBox(shell, SWT.ICON_WARNING | SWT.YES);
		messageBox.setMessage(message);
		messageBox.setText(title);
		messageBox.open();
		shell.dispose();
	}

	private void updateFiltersLayout() {
		for (var filterConfig: filterConfigChain) {
			var btnFilter = new Link(shlFilterParameters, SWT.NONE);
			btnFilter.addSelectionListener(new SelectionAdapter() {
				@Override
				public void widgetSelected(SelectionEvent e) {
				}
			});
			btnFilter.setBounds(200, 50, 100, 20);
			var configId = filterConfig.configId;
			btnFilter.setText("<a>" + configId + "</a>");
		}
	}

	/**
	 * Create contents of the dialog.
	 */
	private void createContents() {
		//shlFilterParameters = new Shell(getParent(), SWT.DIALOG_TRIM);
		shlFilterParameters.setSize(327, 371);
		shlFilterParameters.setText("Cascading filter parameters");

		shlFilterParameters.addListener(SWT.Close, new Listener() {
			public void handleEvent(Event event) {
				//event.doit = false;
			}
		});

		Button btnOk = new Button(shlFilterParameters, SWT.NONE);
		btnOk.addSelectionListener(new SelectionAdapter() {
			@Override
			public void widgetSelected(SelectionEvent e) {
				var filterChain = parameters.getFilterChain();
				filterChain.clear();
				var configIds = parameters.getConfigIds();
				configIds.clear();
				for (var filterConfig : filterConfigChain) {
					var filter = fcMapper.createFilter(filterConfig.configId, null);
					filter.setParameters(filterConfig.parameters);
					filterChain.add(filter);
					configIds.add(filterConfig.configId);
				}
				result = true;
				shlFilterParameters.close();
			}
		});
		btnOk.setBounds(106, 285, 90, 30);
		btnOk.setText("OK");

		Button btnCancel = new Button(shlFilterParameters, SWT.NONE);
		btnCancel.addSelectionListener(new SelectionAdapter() {
			@Override
			public void widgetSelected(SelectionEvent e) {
				result = false;
				shlFilterParameters.close();
			}
		});
		btnCancel.setBounds(202, 285, 90, 30);
		btnCancel.setText("Cancel");

		Button btnHelp = new Button(shlFilterParameters, SWT.NONE);
		btnHelp.addSelectionListener(new SelectionAdapter() {
			@Override
			public void widgetSelected(SelectionEvent e) {
			}
		});
		btnHelp.setGrayed(true);
		btnHelp.setBounds(10, 285, 90, 30);
		btnHelp.setText("Help");

		Button btnTest = new Button(shlFilterParameters, SWT.NONE);
		btnTest.addSelectionListener(new SelectionAdapter() {
			@Override
			public void widgetSelected(SelectionEvent e) {
				showTestResult();
			}
		});
		btnTest.setEnabled(false);

		btnTest.setBounds(202, 177, 90, 30);
		btnTest.setText("Test");

		Button btnAddFile = new Button(shlFilterParameters, SWT.NONE);
		btnAddFile.addSelectionListener(new SelectionAdapter() {
			@Override
			public void widgetSelected(SelectionEvent e) {
				var aPaths = Dialogs.browseFilenames(shlFilterParameters, "Open file", false, "", "", "");
				if (aPaths.length > 0) {
					lblTestFilePath.setText(aPaths[0]);
					btnTest.setEnabled(true);
				} else
					btnTest.setEnabled(false);
			}
		});
		btnAddFile.setBounds(202, 138, 90, 30);
		btnAddFile.setText("Add file");

		swtListFilters = new List(shlFilterParameters, SWT.BORDER);
		swtListFilters.addMouseListener(new MouseAdapter() {
			@Override
			public void mouseDoubleClick(MouseEvent e) {
				int itemTop = 0;
				int index = -1;
				for( int i = 0; i < swtListFilters.getItemCount(); i++ ) {
					if( e.y >= itemTop && e.y <= itemTop + swtListFilters.getItemHeight() ) {
						index = swtListFilters.getTopIndex()  + i;
						System.out.println("Click on item " + swtListFilters.getItem(index));
					}

					itemTop += swtListFilters.getItemHeight();
				}

				if (index > -1) {
					//edit filter
					var filterConfig = filterConfigChain.get(index);
					var filter = fcMapper.createFilter(filterConfig.configId, null);
					filter.setParameters(filterConfig.parameters);

					//var paramEditor = fcMapper.createConfigurationEditor(filterConfig.configId, filter);
					//var editor = new FilterConfigurationEditor();
					IContext context = new BaseContext();
					//IParametersEditor aa = null;
					//fcMapper.edi
					//editor.editConfiguration(filterConfig.configId, fcMapper, filter, shlFilterParameters, context);

					IParametersEditor editor = fcMapper.createConfigurationEditor(filterConfig.configId, filter);
					if ( editor != null ) {
						if ( !editor.edit(filterConfig.parameters, false, context) ) {
							return; // Cancel
						}
					}
					else {
						// Try to see if we can edit with the generic editor
						IEditorDescriptionProvider descProv = fcMapper.getDescriptionProvider(filterConfig.parameters.getClass().getName());
						if ( descProv != null ) {
							// Edit the data
							GenericEditor genEditor = new GenericEditor();
							if ( !genEditor.edit(filterConfig.parameters, descProv, false, context) ) {
								return; // Cancel
							}
							// The params object gets updated if edit not canceled.
						}
						else { // Else: fall back to the plain text editor
							Shell shell = null;
							//if (( parent != null ) && ( parent instanceof Shell )) {
							//	shell = (Shell)parent;
							//}
							InputDialog dlg  = new InputDialog(shell,
									String.format("Filter Parameters (%s)", filterConfig.configId), "Parameters:",
									filterConfig.parameters.toString(), null, 0, 300, 800);
							dlg.setReadOnly(false); // Pre-defined configurations should be read-only
							dlg.changeFontSize(+2);
							String data = dlg.showDialog();
							if ( data == null )
								return; // Cancel
							data = data.replace("\r\n", "\n"); //$NON-NLS-1$ //$NON-NLS-2$
							filterConfig.parameters.fromString(data.replace("\r", "\n")); //$NON-NLS-1$ //$NON-NLS-2$
						}
					}

					// Save the changes here
					//if ( config.custom ) {
					// Save the configuration filefcMapper
					//fcMapper.saveCustomParameters(config, params);
					//}
				}
			}
		});
		swtListFilters.setBounds(10, 22, 166, 185);

		//update the list
		for (var filterConfig: filterConfigChain) {
			swtListFilters.add(filterConfig.configId);
		}


		Button btnAddFilter = new Button(shlFilterParameters, SWT.NONE);
		btnAddFilter.addSelectionListener(new SelectionAdapter() {
			@Override
			public void widgetSelected(SelectionEvent e) {
				var filterConfigDialog = new FilterConfigurationsDialog(shlFilterParameters, true, fcMapper, null);
				var result = filterConfigDialog.showDialog(null);

				if (result != null) {
					//add to the parameters chain
					var filterConfig = fcMapper.getConfiguration(result);
					//load the parameters
					var filter = fcMapper.createFilter(filterConfig.configId);
					filterConfig.parameters = filter.getParameters();
					//reset config id with the base config id
					var pattern = Pattern.compile("(.*)@");
					// Now create matcher object.
					var matcher = pattern.matcher(filterConfig.configId);
					if (matcher.find()) {
						filterConfig.configId = matcher.group(1);
					} else {
						//throw new OkapiException("Invalid config id.");
					}

					filterConfigChain.add(filterConfig);
					swtListFilters.add(filterConfig.configId);
				}
			}
		});
		btnAddFilter.setBounds(202, 21, 90, 30);
		btnAddFilter.setText("Add");

		Button btnRemoveFilter = new Button(shlFilterParameters, SWT.NONE);
		btnRemoveFilter.addSelectionListener(new SelectionAdapter() {
			@Override
			public void widgetSelected(SelectionEvent e) {
				var selectedIdx = swtListFilters.getSelectionIndex();
				if (selectedIdx >= 0)
				{
					if (showConfirmDialog("Confirm", "Are you sure that you want to remove this filter?")) {
						swtListFilters.remove(selectedIdx);
						filterConfigChain.remove(selectedIdx);
					}
				}
			}
		});

		btnRemoveFilter.setBounds(202, 58, 90, 30);
		btnRemoveFilter.setText("Remove");

		Group grpTestFile = new Group(shlFilterParameters, SWT.NONE);
		grpTestFile.setText("Test file");
		grpTestFile.setBounds(10, 222, 282, 50);

		lblTestFilePath = new CLabel(shlFilterParameters, SWT.NONE);
		lblTestFilePath.setBounds(15, 242, 270, 20);
		lblTestFilePath.setText("");
		lblTestFilePath.moveAbove(grpTestFile);
	}

	@Override
	public boolean edit(IParameters params, boolean readOnly, IContext context) {
		if (readOnly) {
			showMessageBox("Warning", "This filter doesn't have default values. Create a custon one.");
			return false;
		}
		//load the parameters
		parameters = (Parameters) params;
		var filterChain =  parameters.getFilterChain();
		var configIds = parameters.getConfigIds();
		for (int i= 0; i < filterChain.size(); i++) {
			var filter = filterChain.get(i);
			var configId =configIds.get(i);
			var filterParams = filter.getParameters();
			var filterConfig = new FilterConfiguration();
			filterConfig.configId = configId;
			filterConfig.parameters = filterParams;
			filterConfigChain.add(filterConfig);
		}

		Display display = null;
		//try {
		shlFilterParameters = new Shell((Shell) context.getObject("shell"),
				SWT.CLOSE | SWT.TITLE | SWT.APPLICATION_MODAL);
		createContents();
		display = shlFilterParameters.getDisplay();
		//} catch (Exception ex) {
		//Dialogs.showError(shlFilterParameters, ex.getLocalizedMessage(), null);
		//result = false;
		//}

		shlFilterParameters.open();
		shlFilterParameters.layout();

		while (!shlFilterParameters.isDisposed()) {
			if (!display.readAndDispatch()) {
				display.sleep();
			}
		}

		return result;
	}

	@Override
	public IParameters createParameters() {
		return null;
	}

	private void showTestResult() {
		BusyIndicator.showWhile(Display.getDefault(), new Runnable() {
			public void run() {
				try {
					// Create the filter object
					IFilter filter = new CascadingFilter();
					filter.setParameters(parameters);

					// Create the RawDocument object
					var filePath = lblTestFilePath.getText();
					File file = new File(filePath);
					RawDocument rawDoc = new RawDocument(new FileInputStream(file), "UTF-8", LocaleId.fromString("en"));
					filter.open(rawDoc);

					//the segmenter
					var classLoader = Editor.class.getClassLoader();
					var inputStream = classLoader.getResourceAsStream("segmentation/All languages segmentation.srx");
					var srxDoc = new SRXDocument();
					srxDoc.loadRules(inputStream);
					ISegmenter segmenter = srxDoc.compileLanguageRules(LocaleId.fromString("en"), null);

					//xliff writer
					var xliffWriter = new XLIFFWriter();
					var sOutXliffPath = filePath + ".xliff";
					xliffWriter.setOutput(sOutXliffPath);
					xliffWriter.setOptions(LocaleId.fromString("fr"), "utf-8");
					var xlfParams = xliffWriter.getParameters();
					xlfParams.setPlaceholderMode(false);
					xlfParams.setPlaceholderMode(false);
					xliffWriter.setParameters(xlfParams);

					// Get the events from the input document
					while ( filter.hasNext() ) {
						net.sf.okapi.common.Event event = filter.next();
						// do something with the event...
						// Here, if the event is TEXT_UNIT, we display the key and the extracted text
						if (event.isTextUnit()) {
							TextUnit tu = (TextUnit)event.getResource();

							//do the segmentation
							var tcSource = tu.getSource();
							var bSegmented = tcSource.hasBeenSegmented();
							int segments = segmenter.computeSegments(tcSource);
							tcSource.getSegments().create(segmenter.getRanges());
						}
						xliffWriter.handleEvent(event);
					}
					// Close the input document
					filter.close();
					xliffWriter.close();

					//open the file in ocelot
					var dropinPath = "C:/Development/OKAPI/ocelot/"; //getDropinsDirectory();
					var ocelotJar = new File(Path.of(dropinPath,"/Ocelot-3.0.jar").toString());

					// Run a java app in a separate system process
					//Process proc = Runtime.getRuntime().exec("java -jar " + ocelotJar + " \"" + sOutXliffPath + "\"");
					// Then retreive the process output
					//var in = proc.getInputStream();
					//var err = proc.getErrorStream();
					var xliffViewer = new XliffViewer(new Shell(), SWT.CLOSE | SWT.TITLE | SWT.APPLICATION_MODAL);
					xliffViewer.open(sOutXliffPath, true);

/*
			URLClassLoader child = new URLClassLoader(
					new URL[]{ocelotJar.toURI().toURL()},
					this.getClass().getClassLoader()
			);

			Class classToLoad = Class.forName("com.vistatec.ocelot.OcelotStarter", true, child);
			Method startMethod = classToLoad.getDeclaredMethod("start");
			Method openFileMethod = classToLoad.getDeclaredMethod("openFile", String.class);
			Object instance = classToLoad.newInstance();
			startMethod.invoke(instance);
			openFileMethod.invoke(instance, sOutXliffPath);
*/

				} catch (Exception ex) {
					System.out.println(ex.toString());
				}
			}
		});
	}
}
