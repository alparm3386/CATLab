using CAT.Enums;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IDocumentService
    {
        public Task<TempDocument> CreateTempDocumentAsync(IFormFile formFile, DocumentType documentType, int filterId);

        public Task<Document> CreateDocumentFromTempDocumentAsync(int tempDocumentId);
        Task<Document> CreateDocumentAsync(byte[] fileContent, string originalFileName, DocumentType documentType);
    }
}
