using okapi.resource;
using System.Data;

namespace CAT.TM
{
    public interface IDataStorage
    {
        void BackupDatabase(string dbName);
        DataSet CheckIncontextMatches(string tmPath, DataTable queryTable);
        int CreateTB(int tbType, int IdType, string[] aLanguages);
        void CreateTranslationMemory(string tmPath);
        bool DBExists(string dbName);
        void DeleteTBEntry(int idTermbase, int idEntry);
        int DeleteTMEntry(string tmPath, int idEntry);
        DataSet GetDbList();
        DataSet GetDbListSyncSource(string backupSourceConnectionString);
        DataSet GetExactMatchesBySource(string tmPath, string source);
        DataSet GetIncontextMatch(string tmPath, string source, string context);
        DataSet GetSourceIndexData(string tmPath);
        DataSet GetSpecialities();
        DataSet GetTBEntry(int idEntry);
        DataSet GetTBInfo(int idTermbase);
        DataSet GetTBInfo(int tbType, int idType);
        DataSet GetTBInfoById(int idTermbase);
        DataSet GetTMEntriesBySourceIds(string tmPath, int[] aIdSource);
        DataSet GetTMEntriesByTargetText(string tmPath, string sTarget);
        DataSet GetTMEntriesFromSyncSource(string backupSourceConnectionString, string dbName);
        int GetTMEntriesNumber(string tmPath);
        DataSet GetTMListFromDatabase(string dbName);
        DataSet GetTranslationMemoryData(string tmPath);
        int InsertTBEntry(int idTermbase, string comment, string user);
        int InsertTerm(int idEntry, KeyValuePair<string, string> term, string user);
        DataSet InsertTMEntry(string tmPath, TextFragment source, TextFragment target, string context, string user, int speciality, int idTranslation, DateTime dateCreated, DateTime dateModified, string extensionData);
        DataSet ListTBEntries(int idTermbase, string[] languages);
        void RemoveTerms(int idTermbase, string langCode);
        void RestoreDatabase(string backupPath, string dbName);
        void SetDatabaseLastAccess(string dbName);
        void ShrinkDatabase(string dbName);
        bool TMExists(string tmPath);
        void UpdateLanguages(int idTermbase, string[] aLanguages);
        void UpdateLastModified(int idTermbase);
        void UpdateTMEntry(string tmPath, int idEntry, Dictionary<string, string> fieldsToUpdate);
    }
}