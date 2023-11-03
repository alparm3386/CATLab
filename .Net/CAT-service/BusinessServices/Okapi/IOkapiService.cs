using CAT.Models;

namespace CAT.BusinessServices.Okapi
{
    public interface IOkapiService
    {     
        string CreateXliffFromDocument(string fileName, byte[] fileContent, string filterName, byte[] filterContent, 
            string sourceLangISO639_1, string targetLangISO639_1);

        byte[] CreateDocumentFromXliff(string fileName, byte[] fileContent, string filterName, byte[] filterContent, 
            string sourceLangISO639_1, string targetLangISO639_1, string xliffContent);

        Statistics[] GetStatisticsForDocument(string sFileName, byte[] fileContent, string sFilterName, byte[] filterContent,
            string sourceLangISO639_1, string[] aTargetLangsISO639_1, TMAssignment[] aTMAssignments);

        string PreTranslateXliff(string sXliffContent, string langFrom_ISO639_1, string langTo_ISO639_1, 
            TMAssignment[] aTMAssignments, int matchThreshold);
    }
}