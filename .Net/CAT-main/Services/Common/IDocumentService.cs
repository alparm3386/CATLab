﻿using CAT.Enums;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IDocumentService
    {
        public Task<TempDocument> CreateTempDocumentAsync(IFormFile formFile, DocumentType documentType, int filterId);

        public Task<Document> CreateDocumentFromTempDocumentAsync(int tempDocumentId);

        Task<Document> CreateDocumentAsync(int jobId, byte[] fileContent, string originalFileName, DocumentType documentType);
        Task<string> GetDocumentFolderAsync(int documentId);
        string GetDocumentFolderForDocumentType(DocumentType documentType);
        DocumentType GetDocumentTypeForTask(Enums.Task taskId);
    }
}
