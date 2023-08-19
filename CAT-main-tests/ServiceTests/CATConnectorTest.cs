using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Xunit;
using CAT.Services.CAT;

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

        [Fact]
        public void TestYourFunctionality()
        {
            var catConnector = new CATConnector(_testFixture.IdentityDbContext, _testFixture.MainDbContext, _testFixture.TranslationUnitsDbContext,
                _testFixture.MockConfiguration.Object, _testFixture.MockedMachineTranslators, _testFixture.MockMapper.Object,
                _testFixture.GetLoggerMockObject<CATConnector>(), _testFixture.MockDocumentProcessor.Object);

            var outBytes = catConnector.CreateDoc(1, 1, false);
            File.WriteAllBytes("C:\\Alpar\\out.docx", outBytes);
        }
    }
}
