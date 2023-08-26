using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Models.Common;
using CAT.Models.Entities.Main;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace CAT.Services.Common
{
    public class DocumentService : IDocumentService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public DocumentService(DbContextContainer dbContextContainer, IConfiguration configuration, 
            IMapper mapper, ILogger<DocumentService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Document> CreateDocumentAsync(IFormFile formFile, DocumentType documentType)
        {
            try
            {
                //save the file into the temp folder
                var tempFolder = _configuration["TempFolder"]!;
                var tempFilePath = Path.Combine(tempFolder, Guid.NewGuid().ToString());
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }

                var sourceFilesFolder = _configuration["SourceFilesFolder"]!;
                var fileName = formFile.FileName;
                var filePath = Path.Combine(sourceFilesFolder, fileName);
                var bSaveFile = true;

                //get the md5 hash
                var md5Hash = "";
                using (var stream = formFile.OpenReadStream())
                {
                    md5Hash = FileHelper.CalculateMD5(tempFilePath);
                }

                if (File.Exists(filePath))
                {
                    var md5Existing = "";
                    using (var stream = formFile.OpenReadStream())
                    {
                        md5Existing = FileHelper.CalculateMD5(filePath);
                    }

                    if (md5Hash == md5Existing)
                        bSaveFile = false;
                    else
                    {
                        // Generate a unique file name based on the original file name
                        fileName = FileHelper.GetUniqueFileName(filePath);
                        filePath = Path.Combine(sourceFilesFolder, fileName);
                    }
                }

                //save the file
                if (bSaveFile)
                {
                    // Combine the unique file name with the server's path to create the full path
                    File.Copy(tempFilePath, filePath);
                }

                //now we can delete the temp file
                File.Delete(tempFilePath);

                //create the document
                var document = new Document()
                {
                    DocumentType = (int)DocumentType.Original,
                    FileName = fileName,
                    OriginalFileName = formFile.FileName,
                    MD5Hash = md5Hash
                };

                await _dbContextContainer.MainContext.Documents.AddAsync(document);
                await _dbContextContainer.MainContext.SaveChangesAsync();

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateDocument ERROR: " + ex.Message);
                throw;
            }
        }
    }
}
