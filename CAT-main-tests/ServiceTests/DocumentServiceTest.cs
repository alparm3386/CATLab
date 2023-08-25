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
        public void Test_CreateDocumentAsync()
        {
            var documentService = new DocumentService(_testFixture.DbContextContainer, _testFixture.MockConfiguration.Object,
                _testFixture.MockMapper.Object, _testFixture.GetLoggerMockObject<DocumentService>());

            // Create a mock instance of IFormFile
            var mockFormFile = new Mock<IFormFile>();

            // Setup the mock properties and methods as needed
            var content = "File content here"; // or any content you want
            var fileName = "test.txt";
            var contentType = "text/plain";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            mockFormFile.Setup(f => f.FileName).Returns(fileName);
            mockFormFile.Setup(f => f.Length).Returns(stream.Length);
            mockFormFile.Setup(f => f.ContentType).Returns(contentType);
            mockFormFile.Setup(f => f.OpenReadStream()).Returns(stream);

            var result = await documentService.CreateDocumentAsync(mockFormFile, Guid.NewGuid().ToString(), false);
            Assert.NotNull(result);
        }
    }
}
