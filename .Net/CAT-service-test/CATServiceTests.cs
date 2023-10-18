using Xunit;
using Grpc.Net.Client;
using CAT_service_test.Utils;
using System.Reflection;
using Xunit.Sdk;
using Proto;
using Google.Protobuf;

namespace CAT
{
    [SkipClass("All tests in this class are skipped")]
    public class CATServiceTests
    {
        private const string ServerAddress = "http://localhost:5082"; // Adjust the address/port as needed


        public CATServiceTests()
        {
        }

        [Fact]
        public async Task TMExists_ReturnsExpectedValue()
        {
            using var channel = GrpcChannel.ForAddress(ServerAddress);
            var client = new Proto.CAT.CATClient(channel);

            try
            {
                //var request = new TMExistsRequest { TmId = "1/_1_en_fr_marketing" };
                //var response = await client.TMExistsAsync(request);
                //var request = new CreateTMRequest { TmId = "1/_1_en_fr" };
                //var response = await client.CreateTMAsync(request);
                //var request = new GetTMInfoRequest { Id = "1/_1_en_fr_marketing", FullInfo = true };
                //var response = await client.GetTMInfoAsync(request);
                //var request = new GetTMListRequest { FullInfo = true };
                //var response = await client.GetTMListAsync(request);
                //var request = new GetTMListFromDatabaseRequest { DbName = "1", FullInfo = true };
                //var response = await client.GetTMListFromDatabaseAsync(request);
                //var request = new GetStatisticsForDocumentRequest
                //{
                //    FileName = "test.txt",
                //    FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Janet Yellen.txt")),
                //    SourceLangISO6391 = "en"
                //};
                //request.TargetLangsISO6391.Add("fr");
                //request.TargetLangsISO6391.Add("de");
                //var response = await client.GetStatisticsForDocumentAsync(request);

                //var request = new PreTranslateXliffRequest { XliffContent = File.ReadAllText("C:\\Alpar\\Test.xlf"), LangFrom = "en", LangTo = "fr" };
                //var response = await client.PreTranslateXliffAsync(request);

                //var request = new GetTMMatchesRequest { SourceText = "test", PrevText = "", NextText = "", MatchThreshold = 50, MaxHits = 10 };
                //var tmAssignment = new Proto.TMAssignment() { Id = "1/_1_en_fr", Penalty = -1, Speciality = 1 };
                //request.TMAssignments.Add(tmAssignment);
                //var response = await client.GetTMMatchesAsync(request);

                //var request = new GetExactMatchRequest { SourceText = "test", PrevText = "", NextText = ""};
                //var tmAssignment = new Proto.TMAssignment() { TmId = "1/_1_en_fr", Penalty = -1, Speciality = 1 };
                //request.TMAssignments.Add(tmAssignment);
                //var response = await client.GetExactMatchAsync(request);

                //var request = new AddTMEntriesRequest { TmId = "1/_1_en_fr" };
                //var tmEntry = new TMEntry()
                //{
                //    Source = "this is a test",
                //    Target = "this is the translation for test",
                //    Metadata = "{\r\n    \"prevSegment\": \"aaa\",\r\n    \"nextSegment\": \"bbb\"\r\n}"
                //};
                //request.TMEntries.Add(tmEntry);
                //var response = await client.AddTMEntriesAsync(request);

                //var request = new DeleteTMEntryRequest { TmId = "1/_1_en_fr", EntryId = 1 };
                //var response = await client.DeleteTMEntryAsync(request);

                //var request = new ConcordanceRequest { SourceText = "test", TargetText = "", CaseSensitive = false, MaxHits = 10 };
                //request.TmIds.Add("1/_1_en_fr");
                //var response = await client.ConcordanceAsync(request);

                //var request = new ImportTmxRequest { TmId = "1/_1_en_fr", SourceLangIso6391 = "en", TargetLangIso6391 = "fr", 
                //    TmxContent = File.ReadAllText("C:\\Alpar\\test fr.tmx"), User = "2104", Speciality = 1 };
                //var response = await client.ImportTmxAsync(request);

                //var request = new CreateTBRequest { TbType = TBType.Profile, IdType = 1 };
                //request.LangCodes.Add(new string[] { "eng", "fre", "ger" });
                //var response = await client.CreateTBAsync(request);

                //var request = new GetTBInfoRequest { TbType = TBType.Profile, IdType = 1 };
                //var response = await client.GetTBInfoAsync(request);
                //var request2 = new GetTBInfoByIdRequest { TermbaseId = 1 };
                //var response2 = await client.GetTBInfoByIdAsync(request2);

                //var request = new AddLanguageToTBRequest { TermbaseId = 1,  LangCode = "spa" };
                //var response = await client.AddLanguageToTBAsync(request);
                //var request2 = new RemoveLanguageFromTBRequest { TermbaseId = 1, LangCode = "spa"};
                //var response2 = await client.RemoveLanguageFromTBAsync(request2);

                var tbEntry = new TBEntry() { Metadata = "" };
                tbEntry.Terms.Add("eng", "eng term");
                tbEntry.Terms.Add("fre", "fre term");
                tbEntry.Terms.Add("ger", "ger term");
                var request = new AddOrUpdateTBEntryRequest { TermbaseId = 1, TbEntry = tbEntry };
                var response = await client.AddOrUpdateTBEntryAsync(request);
                var request2 = new DeleteTBEntryRequest { TermbaseId = 1, EntryId = 1 };
                var response2 = await client.DeleteTBEntryAsync(request2);
                //Assert.True(response);  // Or whatever your expected result is
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
