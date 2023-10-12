using Xunit;
using Grpc.Net.Client;
using CAT_service_test.Utils;
using System.Reflection;
using Xunit.Sdk;

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
            var client = new CAT.CATClient(channel);

            try
            {
                //var request = new TMExistsRequest { TmId = "1/_1_en_fr_marketing" };
                //var response = await client.TMExistsAsync(request);
                var request = new CreateTMRequest { TmId = "1/_1_en_fr_marketing" };
                //var response = await client.CreateTMAsync(request);
                //var request = new TMInfoRequest { Id = "1/_1_en_fr_marketing", FullInfo = false };
                var response = await client.GetTMInfoAsync(request);

                //Assert.True(response);  // Or whatever your expected result is
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
