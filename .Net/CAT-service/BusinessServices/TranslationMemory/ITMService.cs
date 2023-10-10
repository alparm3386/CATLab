using CAT.Enums;
using CAT.Models;

namespace CAT.TM
{
    public interface ITMService
    {
        int AddTMEntries(string tmPath, TMEntry[] tmEntries);
        TMEntry[] Concordance(string[] tmIds, string sSourceText, string sTargetText, bool bCaseSensitive, int maxHits);
        string ConnectionPoolInfo();
        void CreateTM(string tmId);
        void DeleteDocumentsFromSourceIndex(int[] aDocIds);
        void DeleteTMEntry(string tmPath, int idEntry);
        string ExportTmx(string tmPath);
        TMMatch GetExactMatch(TMAssignment[] aTmAssignments, string source, string prev, string next);
        Statistics[] GetStatisticsForDocument(string sFileName, byte[] fileContent, string sFilterName, byte[] filterContent, string sourceLangISO639_1, string[] aTargetLangsISO639_1, TMAssignment[] aTMAssignments);
        TMInfo GetTMInfo(string id, bool fullInfo);
        TMInfo[] GetTMList(bool bFullInfo);
        TMInfo[] GetTMListFromDatabase(string dbName);
        TMMatch[] GetTMMatches(TMAssignment[] aTMAssignments, string sSourceText, string sPrevText, string sNextText, byte matchThreshold, int maxHits);
        TMImportResult ImportTmx(string tmPath, string sSourceLangIso639_1, string sTargetLangIso639_1, string sTMXContent, string sUser, int speciality);
        string PreTranslateXliff(string sXliffContent, string langFrom_ISO639_1, string langTo_ISO639_1, TMAssignment[] aTMAssignments, int matchThreshold);
        int ReindexTM(string tmId, TMIndex index);
        void ShrinkTM(string tmPath);
        bool TMExists(string tmId);
        void UpdateTMEntry(string tmPath, int idEntry, Dictionary<string, string> fieldsToUpdate);
    }
}