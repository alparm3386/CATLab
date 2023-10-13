using CAT.Enums;
using CAT.Models;

namespace CAT.TM
{
    public interface ITMService
    {
        bool TMExists(string tmId);

        void CreateTM(string tmId);

        TMInfo GetTMInfo(string id, bool fullInfo);

        TMInfo[] GetTMList(bool bFullInfo);

        TMInfo[] GetTMListFromDatabase(string dbName, bool bFullInfo);

        Statistics[] GetStatisticsForDocument(string sFileName, byte[] fileContent, string sFilterName, byte[] filterContent, string sourceLangISO639_1, string[] aTargetLangsISO639_1, TMAssignment[] aTMAssignments);

        string PreTranslateXliff(string sXliffContent, string langFrom_ISO639_1, string langTo_ISO639_1, TMAssignment[] aTMAssignments, int matchThreshold);

        TMMatch[] GetTMMatches(TMAssignment[] aTMAssignments, string sSourceText, string sPrevText, string sNextText, byte matchThreshold, int maxHits);

        TMMatch GetExactMatch(TMAssignment[] aTmAssignments, string source, string prev, string next);

        int AddTMEntries(string tmPath, TMEntry[] tmEntries);

        void DeleteTMEntry(string tmPath, int idEntry);

        TMEntry[] Concordance(string[] tmIds, string sSourceText, string sTargetText, bool bCaseSensitive, int maxHits);

        string ExportTmx(string tmPath);

        TMImportResult ImportTmx(string tmPath, string sSourceLangIso639_1, string sTargetLangIso639_1, string sTMXContent, string sUser, int speciality);

        int ReindexTM(string tmId, TMIndex index);

        void ShrinkTM(string tmPath);

        void UpdateTMEntry(string tmPath, int idEntry, Dictionary<string, string> fieldsToUpdate);

        string ConnectionPoolInfo();
    }
}