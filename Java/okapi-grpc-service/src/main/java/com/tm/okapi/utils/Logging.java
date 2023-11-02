package com.tm.okapi.utils;

import java.io.IOException;
import java.nio.file.*;

import javax.naming.*;

/**
 * @author Alpar
 *
 */
public class Logging {
	private static String LOG_FOLDER = "/data/CAT/Log";

	public synchronized static void Log(String sFileName, String sMsg) {
		Log(sFileName, sMsg, false);
	}

	public synchronized static void Log(String sFileName, String sMsg, boolean bSeparator) {
		try {
			Files.write(Paths.get(LOG_FOLDER, sFileName), (sMsg+ "\r\n").getBytes(), 
					StandardOpenOption.CREATE, StandardOpenOption.APPEND);
		} catch (IOException e) {
			// exception handling left as an exercise for the reader
		}
	}
}