using CAT.Areas.Identity.Data;
using CAT.Data;
using CAT.Models.Entities.Main;
using Microsoft.EntityFrameworkCore;

public class TestFixture
{
    public IdentityDbContext IdentityDbContext { get; }
    public MainDbContext MainDbContext { get; }
    // ... other mocks or contexts

    public TestFixture()
    {
        var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "IdentityInMemoryDb")
            .Options;

        IdentityDbContext = new IdentityDbContext(identityOptions);

        var mainOptions = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: "MainInMemoryDb")
            .Options;

        MainDbContext = new MainDbContext(mainOptions);

        SeedDbContextsWithSampleData();
        // ... setup other mocks or contexts
    }

    // You can also provide methods to setup specific behaviors or seed data
    public void SeedDbContextsWithSampleData()
    {
        //the identity db...

        //the main db
        var document = new Document()
        {
            OriginalFileName = "testFile.txt",
            FileName = "testFile_" + DateTime.Now + ".txt",
            FilterId = -1,
            DocumentType = 0,
            AnalisysId = -1
        };

        // Save the job
        MainDbContext.Documents.Add(document);
        MainDbContext.SaveChanges();

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
        MainDbContext.Orders.Add(order);

        //Save the quote
        MainDbContext.Quotes.Add(quote);

        // Save the job
        MainDbContext.Jobs.Add(job);
        MainDbContext.SaveChanges();

        //the translation units db
    }

    // ... any other utility methods that might help in seeding data or other configurations
}
