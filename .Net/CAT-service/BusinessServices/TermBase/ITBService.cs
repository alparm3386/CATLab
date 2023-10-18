using CAT.Enums;
using CAT.Models;

namespace CAT.TB
{
    public interface ITBService
    {
        TBInfo CreateTB(TBType tbType, int idType, string[] langCodes);
        TBInfo GetTBInfo(TBType tbType, int idType);
        TBInfo GetTBInfoById(int termbaseId);
        void AddLanguageToTB(int termbaseId, string langCode);
        int AddOrUpdateTBEntry(int termbaseId, TBEntry tbEntry);
        void DeleteTBEntry(int termbaseId, int idEntry);
        TBImportResult ImportTB(int termbaseId, string sCsvContent, string sUserId);
        TBImportResult ImportTBEntries(int termbaseId, TBEntry[] tbEntries);
        TBEntry[] ListTBEntries(int termbaseId, string[] languages);
        void RemoveLanguageFromTB(int termbaseId, string langCode);
    }
}