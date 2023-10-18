using CAT.Enums;
using CAT.Models;

namespace CAT.TB
{
    public interface ITBService
    {
        TBInfo CreateTB(TBType tbType, int idType, string[] langCodes);
        TBInfo GetTBInfo(TBType tbType, int idType);
        TBInfo GetTBInfo(int termbaseId);
        void AddLanguageToTB(int termbaseId, string langCode);
        void RemoveLanguageFromTB(int termbaseId, string langCode);
        int AddOrUpdateTBEntry(int termbaseId, TBEntry tbEntry, String user);
        void DeleteTBEntry(int termbaseId, int entryId);
        TBImportResult ImportTB(int termbaseId, string sCsvContent, string user);
        TBImportResult ImportTBEntries(int termbaseId, TBEntry[] tbEntries);
        TBEntry[] ListTBEntries(int termbaseId, string[] languages);
    }
}