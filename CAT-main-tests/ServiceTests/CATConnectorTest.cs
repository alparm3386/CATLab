using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Xunit;
using CAT.Services.Common;

namespace YourNamespace.Tests
{
    public class CATConnectorTest : IClassFixture<TestFixture>
    {
        //private readonly CATConnector _catConnector;
        private TestFixture _testFixture;

        public CATConnectorTest(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        //[Fact]
        public void Test_CreateDoc()
        {
            var catConnector = new CATConnector(_testFixture.DbContextContainer, _testFixture.MockConfiguration.Object, 
                _testFixture.MockedMachineTranslators, _testFixture.MockMapper.Object, _testFixture.GetLoggerMockObject<CATConnector>(), 
                _testFixture.MockDocumentProcessor.Object);

            var fileData = catConnector.CreateDoc(1, Guid.NewGuid().ToString(), false);
            File.WriteAllBytes("C:\\Alpar\\out.docx", fileData.Content!);
        }
    }
}
