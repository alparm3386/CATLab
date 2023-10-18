using CAT.Enums;
using CAT.Models;

namespace CAT.TB
{
    public interface ITBService
    {
        TBInfo CreateTB(TBType tbType, int idType, string[] langCodes);
        TBInfo GetTBInfo(TBType tbType, int idType);
        TBInfo GetTBInfoById(int idTermbase);
        void AddLanguageToTB(int idTermbase, string langCode);
        int AddOrUpdateTBEntry(int idTermbase, TBEntry tbEntry);
        void DeleteTBEntry(int idTermbase, int idEntry);
        TBImportResult ImportTB(int idTermbase, string sCsvContent, string sUserId);
        TBImportResult ImportTBEntries(int idTermbase, TBEntry[] tbEntries);
        TBEntry[] ListTBEntries(int idTermbase, string[] languages);
        void RemoveLanguageFromTB(int idTermbase, string langCode);
    }
}