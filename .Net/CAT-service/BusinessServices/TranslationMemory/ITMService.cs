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

        Statistics[] GetStatisticsForDocument(string sFileName, byte[] fileContent, string sFilterName, byte[] filterContent, 
            string sourceLangISO639_1, string[] aTargetLangsISO639_1, TMAssignment[] aTMAssignments);

        string PreTranslateXliff(string sXliffContent, string langFrom_ISO639_1, string langTo_ISO639_1, 
            TMAssignment[] aTMAssignments, int matchThreshold);

        TMMatch[] GetTMMatches(TMAssignment[] aTMAssignments, string sourceText, string prevText, string nextText, byte matchThreshold, int maxHits);

        TMMatch GetExactMatch(TMAssignment[] aTmAssignments, string source, string prev, string next);

        int AddTMEntries(string tmId, TMEntry[] tmEntries);

        void DeleteTMEntry(string tmId, int entryId);

        TMEntry[] Concordance(string[] tmIds, string sourceText, string targetText, bool caseSensitive, int maxHits);

        string ExportTmx(string tmId);

        TMImportResult ImportTmx(string tmId, string sourceLangIso639_1, string targetLangIso639_1, string tmxContent, string user, int speciality);

        int ReindexTM(string tmId, TMIndex index);

        void ShrinkTM(string tmId);

        void UpdateTMEntry(string tmId, int idEntry, Dictionary<string, string> fieldsToUpdate);

        string ConnectionPoolInfo();
    }
}