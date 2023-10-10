using CAT.Enums;
using CAT.Models;

namespace CAT.TB
{
    public interface ITBService
    {
        void AddLanguageToTB(int idTermbase, string langCode);
        int AddOrUpdateTBEntry(int idTermbase, TBEntry tbEntry);
        TBInfo CreateTB(TBType tbType, int idType, string[] langCodes);
        void DeleteTBEntry(int idTermbase, int idEntry);
        TBInfo GetTBInfo(TBType tbType, int idType);
        TBInfo GetTBInfoById(int idTermbase);
        TBImportResult ImportTB(int idTermbase, string sCsvContent, string sUserId);
        TBImportResult ImportTBEntries(int idTermbase, TBEntry[] tbEntries);
        TBEntry[] ListTBEntries(int idTermbase, string[] languages);
        void RemoveLanguageFromTB(int idTermbase, string langCode);
    }
}