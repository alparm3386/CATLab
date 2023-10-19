namespace CAT.BusinessServices.Okapi
{
    public interface IOkapiConnector
    {
        byte[] CreateDocumentFromXliff(string fileName, byte[] fileContent, string filterName, byte[] filterContent, string sourceLangISO639_1, string targetLangISO639_1, string xliffContent);
        string CreateXliffFromDocument(string fileName, byte[] fileContent, string filterName, byte[] filterContent, string sourceLang, string targetLang);
    }
}