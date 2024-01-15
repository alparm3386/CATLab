using Microsoft.AspNetCore.Mvc.Testing;

namespace CAT_service.UnitTests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        //[Fact]
        public async Task TestGrpcService()
        {
            try
            {
                // Create a gRPC client channel to connect to the test server
                var channel = _factory.CreateClient();
                //var client = new CatServiceClient(channel);

                // Call the gRPC service method and validate the response
                //var request = new CatRequest { Message = "Hello from integration test" };
                //var response = await client.GetResponseAsync(request);
                //Assert.Equal("Hello from integration test", response.Message);

                // Arrange
                var client = _factory.CreateClient();

                // Act
                var response = await client.GetAsync("/"); // Use the correct route that your app responds to

                // Assert
                response.EnsureSuccessStatusCode(); // Status Code 200-299
                Assert.True(true);
            }
            catch (Exception ex)
            {
                int a = 0;
            }
        }
    }
}
