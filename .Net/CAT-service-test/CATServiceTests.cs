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

            var request = new TMExistsRequest { TmId = "_1/1_en_fr_marketing" };

            try
            {
                var response = await client.TMExistsAsync(request);
                Assert.True(response.Exists);  // Or whatever your expected result is
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
