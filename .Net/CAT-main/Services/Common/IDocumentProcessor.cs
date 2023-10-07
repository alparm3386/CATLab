namespace CAT.Services.Common
{
    public interface IDocumentProcessor
    {
        string PreProcessDocument(string filePath, string filterPath);
    }
}
