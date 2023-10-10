namespace CAT.BusinessServices.Okapi
{
    public interface IOkapiConnector
    {
        byte[] CreateDocumentFromXliff(string sFileName, byte[] fileContent, string sFilterName, byte[] filterContent, string sourceLangISO639_1, string targetLangISO639_1, string sXliffContent);
        string CreateXliffFromDocument(string sFileName, byte[] fileContent, string sFilterName, byte[] filterContent, string sourceLang, string targetLang);
    }
}