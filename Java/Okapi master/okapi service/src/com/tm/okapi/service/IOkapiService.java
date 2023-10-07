package com.tm.okapi.service;

import java.util.AbstractMap;
import java.util.Date;
import java.util.HashMap;

import com.tm.okapi.service.OkapiService.*;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
public interface IOkapiService
{
	// TODO: Add your service operations here
	Statistics[] getStatisticsForDocument(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent, String sourceLangISO639_1,
			String[] aTargetLangsISO639_1, TMAssignment[] aTMAssignments) throws Exception;

	byte[] createDocumentFromXliff(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent, String sourceLangISO639_1,
			String targetLangISO639_1, String sXliffContent) throws Exception;

	String createXliff(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent, String sourceLangISO639_1, String targetLangISO639_1,
			TMAssignment[] aTMAssignments) throws Exception;

	boolean tmExists(String sTMName);

	boolean createTM(String sTMName, String sSourceLangIso639_1, String sTargetLangIso639_1);

	TMMatch[] getTMMatches(String[] aTMNames, String sSourceText, String sPrevText, 
			String sNextText, byte matchThreshold, int maxHits);

	int addTMEntries(String sTMName, TMEntry[] tmEntries) throws Exception;

	TMImportResult importTMX(String sTMName, String sTMXContent) throws Exception;
	
	String exportTMX(String sTMName) throws Exception;

	void deleteTMEntry(String sTMName, int idEntry) throws Exception;

	void updateTMEntry(String sTMName, int idEntry, String jsonData) throws Exception;

	void setTMEntryCustomField(String sTMName, int key, String fieldName, String value) throws Exception;

	CorpusEntry[] concordance(String[] aTMNames, String sSourceText, String sTargetText, boolean bCaseSensitive, boolean bNumericEquivalenve, int nLimit)
			throws Exception;

	TMInfo[] getTMList(String sNameContains) throws Exception;

	TMInfo getTMInfo(String sTMName) throws Exception;

	String preTranslateXliff(String sXliffContent, String sourceLangISO639_1, String targetLangISO639_1, TMAssignment[] aTMAssignments, int matchThreshold)
			throws Exception;

	Date getTMLastUpdated(String sTMName) throws Exception;

	RepositoryInfo getRepositoryInfo() throws Exception;
}
