using CAT.Enums;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IDocumentService
    {
        Task<Document> CreateDocumentAsync(IFormFile formFile, DocumentType documentType);
        Task<TempDocument> CreateTempDocumentAsync(IFormFile formFile, DocumentType documentType, int filterId);

    }
}
