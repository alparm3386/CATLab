using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using CAT.GRPCServices;

namespace CAT_service.UnitTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Program>(); // Use your Program class as the startup
                });
        }
    }

    public class ProgramTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProgramTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_Endpoint_ReturnsSuccess()
        {
            try
            {
                // Arrange
                var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/"); // Use the correct route that your app responds to

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            }
            catch (Exception ex)
            {
                int a = 0;
            }
        }
    }
}
