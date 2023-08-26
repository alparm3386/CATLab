using AutoMapper;
using CAT.Areas.Identity.Data;
using CAT.Data;
using CAT.Models.Entities.Main;
using CAT.Models.Entities.TranslationUnits;
using CAT.Services.Common;
using CAT.Services.MT;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Cryptography.Xml;

public class TestFixture
{
    //databases
    public DbContextContainer DbContextContainer { get; }

    //configuration
    public Mock<IConfiguration> MockConfiguration { get; }

    //machine translators
    public IEnumerable<IMachineTranslator> MockedMachineTranslators { get; private set; }

    // ... other mocks
    public Mock<IMapper> MockMapper { get; }

    public Mock<ILogger<CATConnector>> MockLogger { get; }

    public Mock<IDocumentProcessor> MockDocumentProcessor { get; }

    public TestFixture()
    {
        //identity
        var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "IdentityInMemoryDb")
            .Options;

        var identityDbContext = new IdentityDbContext(identityOptions);

        //main
        var mainOptions = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: "MainInMemoryDb")
            .Options;
        var mainDbContext = new MainDbContext(mainOptions);

        //translation units
        var translationUnitsOptions = new DbContextOptionsBuilder<TranslationUnitsDbContext>()
            .UseInMemoryDatabase(databaseName: "TranslationUnitsInMemoryDb")
            .Options;
        var translationUnitsDbContext = new TranslationUnitsDbContext(translationUnitsOptions);

        DbContextContainer = new DbContextContainer(identityDbContext, mainDbContext, translationUnitsDbContext);

        //seed the data
        SeedDbContextsWithSampleData();

        // Set up the mocked IConfiguration
        MockConfiguration = new Mock<IConfiguration>();
        MockConfiguration.SetupGet(m => m["SourceFilesFolder"]).Returns("C:/Development/CATLab/Contents/SourceFiles");
        MockConfiguration.SetupGet(m => m["FileFiltersFolder"]).Returns("C:/Development/CATLab/Contents/Filters");
        MockConfiguration.SetupGet(m => m["JobDataBaseFolder"]).Returns("C:/Development/CATLab/Contents/JobData");
        MockConfiguration.SetupGet(m => m["TempFolder"]).Returns("C:/Development/CATLab/Contents/TempFolder");

        // Mocking a section
        //var databaseSectionMock = new Mock<IConfigurationSection>();
        //databaseSectionMock.Setup(a => a["ConnectionString"]).Returns("YourConnectionString");
        //MockConfiguration.Setup(a => a.GetSection("Database")).Returns(databaseSectionMock.Object);

        //machine translators
        var mockMMT = new Mock<IMachineTranslator>();
        mockMMT.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
                      .Returns<string, string, string, object?>((sText, sFrom, sTo, mtParams) => sText + "_translation to" + sTo);

        // Step 2: Create a list and add the mocked instances to it.
        MockedMachineTranslators = new List<IMachineTranslator>
        {
            mockMMT.Object
        };

        // Mocking IMapper
        MockMapper = new Mock<IMapper>();
        MockMapper.Setup(mapper => mapper.Map<It.IsAnyType>(It.IsAny<object>()))
                  .Returns(new InvocationFunc(invocation => invocation.Arguments[0]));

        // Mocking DocumentProcessor
        MockDocumentProcessor = new Mock<IDocumentProcessor>();
        MockDocumentProcessor.Setup(dp => dp.PreProcessDocument(It.IsAny<string>(), It.IsAny<string>()))
                                     .Returns((string filePath, string filterPath) => null!);
    }

    // You can also provide methods to setup specific behaviors or seed data
    public void SeedDbContextsWithSampleData()
    {
        //the identity db...

        //the main db
        var document = new Document()
        {
            OriginalFileName = "Janet Yellen_small.docx",
            FileName = "Janet Yellen_small_20230817075647681.docx",
            DocumentType = 0
        };

        // Save the job
        DbContextContainer.MainContext.Documents.Add(document);
        DbContextContainer.MainContext.SaveChanges();

        var quote = new Quote()
        {
            SourceLanguage = "en",
            TargetLanguage = "fr",
            Fee = 10.0,
            DateCreated = DateTime.Now,
            Speciality = 0
        };

        var order = new Order()
        {
            DateCreated = DateTime.Now,
            ClientId = 0,
        };

        var job = new Job()
        {
            Quote = quote,
            Order = order,
            SourceDocumentId = document.Id
        };

        //Save the order
        DbContextContainer.MainContext.Orders.Add(order);

        //Save the quote
        DbContextContainer.MainContext.Quotes.Add(quote);

        // Save the job
        DbContextContainer.MainContext.Jobs.Add(job);
        DbContextContainer.MainContext.SaveChanges();

        //the translation units db
        var tuLines = File.ReadAllLines("translationUnitsData.txt");
        var tus = new List<TranslationUnit>();
        for (int i = 0; i < tuLines.Length; i++)
        {
            var tuLine = tuLines[i]; 
            var tuFields = tuLine.Split('\t');
            tus.Add(new TranslationUnit()
            {
                idJob = 1,
                source = tuFields[1],
                target = tuFields[4],
                tuid = i + 1
            });
        }
        DbContextContainer.TranslationUnitsContext.TranslationUnit.AddRange(tus);
        DbContextContainer.TranslationUnitsContext.SaveChanges();
    }

    // ... any other utility methods that might help in seeding data or other configurations
    public ILogger<T> GetLoggerMockObject<T>()
    {
        return CreateLoggerMock<T>().Object;
    }

    private Mock<ILogger<T>> CreateLoggerMock<T>()
    {
        return new Mock<ILogger<T>>();
    }
}
