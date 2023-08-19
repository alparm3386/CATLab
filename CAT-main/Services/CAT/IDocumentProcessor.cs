namespace CAT.Services.CAT
{
    public interface IDocumentProcessor
    {
        string PreProcessDocument(string filePath, string filterPath);
    }
}
