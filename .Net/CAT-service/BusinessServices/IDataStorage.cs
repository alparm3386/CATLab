using CAT.Okapi.Resources;
using System.Data;

namespace CAT.BusinessServices
{
    public interface IDataStorage
    {
        void BackupDatabase(string dbName);
        DataSet CheckIncontextMatches(string tmId, DataTable queryTable);
        int CreateTB(int tbType, int IdType, string[] aLanguages);
        void CreateTranslationMemory(string tmId);
        bool DBExists(string dbName);
        void DeleteTBEntry(int idTermbase, int idEntry);
        int DeleteTMEntry(string tmId, int idEntry);
        DataSet GetDbList();
        DataSet GetDbListSyncSource(string backupSourceConnectionString);
        DataSet GetExactMatchesBySource(string tmId, string source);
        DataSet GetIncontextMatch(string tmId, string source, string context);
        DataSet GetSourceIndexData(string tmId);
        DataSet GetSpecialities();
        DataSet GetTBEntry(int idEntry);
        DataSet GetTBInfo(int idTermbase);
        DataSet GetTBInfo(int tbType, int idType);
        DataSet GetTMEntriesBySourceIds(string tmId, int[] aIdSource);
        DataSet GetTMEntriesByTargetText(string tmId, string sTarget);
        DataSet GetTMEntriesFromSyncSource(string backupSourceConnectionString, string dbName);
        int GetTMEntriesNumber(string tmId);
        DataSet GetTMListFromDatabase(string dbName);
        DataSet GetTranslationMemoryData(string tmId);
        int InsertTBEntry(int idTermbase, string user);
        int InsertTerm(int idEntry, KeyValuePair<string, string> term, string user);
        DataSet InsertTMEntry(string tmId, TextFragment source, TextFragment target, string context, string user, int speciality, int idTranslation, DateTime dateCreated, DateTime dateModified, string extensionData);
        DataSet ListTBEntries(int idTermbase, string[] languages);
        void RemoveTerms(int idTermbase, string langCode);
        void RestoreDatabase(string backupPath, string dbName);
        void SetDatabaseLastAccess(string dbName);
        void ShrinkDatabase(string dbName);
        bool TMExists(string tmId);
        void UpdateLanguages(int idTermbase, string[] aLanguages);
        void UpdateLastModified(int idTermbase);
        void UpdateTMEntry(string tmId, int idEntry, Dictionary<string, string> fieldsToUpdate);
    }
}