package com.tm.okapi.service;

import java.util.AbstractMap;
import java.util.Date;
import java.util.HashMap;

import com.tm.okapi.service.OkapiService.*;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
public interface IOkapiService
{
	String createXliffFromDocument(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent, String sourceLangISO639_1, 
			String targetLangISO639_1) throws Exception;

	byte[] createDocumentFromXliff(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent, String sourceLangISO639_1,
			String targetLangISO639_1, String sXliffContent) throws Exception;
}
