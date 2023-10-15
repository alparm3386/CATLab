using CAT.Models;

namespace CAT.BusinessServices.Okapi
{
    public interface IOkapiService
    {
        byte[] CreateDocumentFromXliff(string sFileName, byte[] fileContent, string sFilterName, byte[] filterContent, string sourceLangISO639_1, string targetLangISO639_1, string sXliffContent);
        string CreateXliffFromDocument(string sFileName, byte[] fileContent, string sFilterName, byte[] filterContent, string sourceLangISO639_1, string targetLangISO639_1, TMAssignment[] aTMAssignments);
        string PreTranslateXliff(string sXliffContent, string langFrom_ISO639_1, string langTo_ISO639_1, TMAssignment[] aTMAssignments, int matchThreshold);
    }
}