using CAT.BusinessServices;
using CAT.Infrastructure;
using CAT.TM;
using CAT.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CAT_service.UnitTests.BusinessServices.TranslationMemory
{
    public class TMServiceTests
    {
        private readonly TMService _tmService;
        private readonly Mock<IDataStorage> _mockDataStorage;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<ILogger<ITMService>> _mockLogger;
        private readonly string _tmPath = "test-path";

        public TMServiceTests()
        {
            _mockDataStorage = new Mock<IDataStorage>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockLogger = new Mock<ILogger<ITMService>>();

            // Setup configuration or other dependencies as needed

            _mockConfiguration.Setup(c => c["TMPath"]).Returns(_tmPath);

            _tmService = new TMService(_mockDataStorage.Object, _mockConfiguration.Object, _mockFileSystem.Object, _mockLogger.Object);
        }

        [Fact]
        public void TMExists_ShouldTrowExcepton_ForInvalidTMId()
        {
            // Arrange
            var tmId = "test-tm-id";

            // Act
            // Act and Assert
            var exception = Assert.Throws<CATException>(() => _tmService.TMExists(tmId));

            // Assert the exception message
            Assert.Equal("Invalid TM id.", exception.Message);
        }

        [Fact]
        public void TMExists_ShouldReturnFalse_WhenTMDirectoryDoesNotExist()
        {
            // Arrange
            var tmId = "1/_1_en_fr";
            _mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

            // Act
            var result = _tmService.TMExists(tmId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TMExists_ShouldReturnTrue_WhenTMDirectoryExists()
        {
            // Arrange
            var tmId = "1/_1_en_fr";
            _mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

            // Act
            var result = _tmService.TMExists(tmId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TMExists_ShouldReturnFalse_WhenTMDirectoryDoesNotExistInDb()
        {
            // Arrange
            var tmId = "1/_1_en_fr";
            _mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _mockDataStorage.Setup(ds => ds.TMExists(It.IsAny<string>())).Returns(false);

            // Act
            var result = _tmService.TMExists(tmId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TMExists_ShouldReturnTrue_WhenTMDirectoryExistsInDb()
        {
            // Arrange
            var tmId = "1/_1_en_fr";
            _mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _mockDataStorage.Setup(ds => ds.TMExists(It.IsAny<string>())).Returns(true);

            // Act
            var result = _tmService.TMExists(tmId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TMExists_ShouldReturnFalse_WhenTMIndexDirectoryDoesNotExist()
        {
            // Arrange
            var tmId = "1/_1_en_fr";
            _mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _mockDataStorage.Setup(ds => ds.TMExists(It.IsAny<string>())).Returns(true);

            // Act
            var result = _tmService.TMExists(tmId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TMExists_ShouldReturnTrue_WhenTMExists()
        {
            // Arrange
            var tmId = "1/_1_en_fr";

            _mockFileSystem
                .Setup(fs => fs.DirectoryExists(Path.Combine(_tmPath, "1")))
                .Returns(true); // Return true when the path matches

            _mockDataStorage.Setup(ds => ds.TMExists(It.IsAny<string>())).Returns(true);

            _mockFileSystem
                .Setup(fs => fs.DirectoryExists(Path.Combine(_tmPath, "1\\source indexes/_1_en_fr")))
                .Returns(true); 

            // Act
            var result = _tmService.TMExists(tmId);

            // Assert
            Assert.True(result);
        }

        //[Fact]
        //public void CreateTM_WhenTMExists_DoesNotCreateNewTM()
        //{
        //    // Arrange
        //    var tmId = "existingTM";
        //    _mockDataStorage.Setup(ds => ds.TMExists(tmId)).Returns(true);

        //    // Act
        //    _tmService.CreateTM(tmId);

        //    // Assert
        //    _mockDataStorage.Verify(ds => ds.CreateTranslationMemory(It.IsAny<string>()), Times.Never);
        //}

        //[Fact]
        //public void CreateTM_WhenTMDoesNotExist_CreatesNewTM()
        //{
        //    // Arrange
        //    var tmId = "newTM";
        //    _mockDataStorage.Setup(ds => ds.TMExists(tmId)).Returns(false);

        //    // Act
        //    _tmService.CreateTM(tmId);

        //    // Assert
        //    _mockDataStorage.Verify(ds => ds.CreateTranslationMemory(tmId), Times.Once);
        //}
    }
}
