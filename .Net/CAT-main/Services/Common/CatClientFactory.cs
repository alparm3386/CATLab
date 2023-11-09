using Grpc.Net.Client;
using static Proto.CAT;

namespace CAT.Services.Common
{
    public class CatClientFactory
    {
        private readonly Lazy<GrpcChannel> _channel;

        public CatClientFactory(IConfiguration configuration)
        {
            _channel = new Lazy<GrpcChannel>(() =>
            {
                // Create and configure the channel here
                return GrpcChannel.ForAddress(configuration!["CATServer"]!);
            });
        }

        public CATClient CreateClient()
        {
            return new CATClient(_channel.Value);
        }
    }

}
