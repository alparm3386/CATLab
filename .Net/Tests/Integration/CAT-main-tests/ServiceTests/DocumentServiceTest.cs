using CAT.Services.Common;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT_main_tests.ServiceTests
{
    public class DocumentServiceTest : IClassFixture<TestFixture>
    {
        //private readonly CATConnector _catConnector;
        private TestFixture _testFixture;

        public DocumentServiceTest(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact]
        public async Task Test_CreateDocumentAsync()
        {
            var documentService = new DocumentService(_testFixture.DbContextContainer, _testFixture.MockConfiguration.Object,
                _testFixture.MockLanguageService.Object, _testFixture.MockMapper.Object, _testFixture.GetLoggerMockObject<DocumentService>());

            // Create a mock instance of IFormFile
            var mockFormFile = new Mock<IFormFile>();

            // Setup the mock properties and methods as needed
            var content = "File content here updated"; // or any content you want
            var fileName = "test.txt";
            var contentType = "text/plain";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            mockFormFile.Setup(f => f.FileName).Returns(fileName);
            mockFormFile.Setup(f => f.Length).Returns(stream.Length);
            mockFormFile.Setup(f => f.ContentType).Returns(contentType);
            mockFormFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFormFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                        .Returns((Stream targetStream, CancellationToken? cancellationToken) => stream.CopyToAsync(targetStream));

            var result = await documentService.CreateTempDocumentAsync(mockFormFile.Object, CAT.Enums.DocumentType.Original, -1);
            Assert.NotNull(result);
        }
    }
}
