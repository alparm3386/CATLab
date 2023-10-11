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
        private const string ServerAddress = "https://localhost:5001"; // Adjust the address/port as needed


        public CATServiceTests()
        {
        }
        
        [Fact]
        public async Task TMExists_ReturnsExpectedValue()
        {
            using var channel = GrpcChannel.ForAddress(ServerAddress);
            var client = new CAT.CATClient(channel);

            var request = new TMExistsRequest { TmId = "test123" };

            var response = await client.TMExistsAsync(request);

            Assert.True(response.Exists);  // Or whatever your expected result is
        }
    }
}
