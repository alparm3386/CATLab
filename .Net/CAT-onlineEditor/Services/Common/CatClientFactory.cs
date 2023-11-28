using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using static Proto.CAT;
using System;

namespace CAT.Services.Common
{
    public class CatClientFactory : ICatClientFactory
    {
        private Lazy<GrpcChannel> _channel = default!;
        private readonly IConfiguration _configuration = default!;

        public CatClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
            InitializeChannel();
        }

        private void InitializeChannel()
        {
            _channel = new Lazy<GrpcChannel>(() =>
            {
                // Create and configure the channel here
                return GrpcChannel.ForAddress(_configuration!["CATServer"]!);
            });
        }

        public CATClient CreateClient()
        {
            return new CATClient(_channel.Value);
        }

        public void ResetChannel()
        {
            // Dispose of the current _channel
            _channel.Value.Dispose();

            // Initialize a new _channel with the provided configuration
            InitializeChannel();
        }
    }
}
