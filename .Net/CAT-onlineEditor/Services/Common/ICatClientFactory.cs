namespace CAT.Services.Common
{
    public interface ICatClientFactory
    {
        Proto.CAT.CATClient CreateClient();
        void ResetChannel();
    }
}