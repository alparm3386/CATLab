namespace CAT.Services.Common
{
    public interface IMonitoringService
    {
        Task<dynamic> GetMonitoringData(DateTime dateFrom, DateTime dateTo);
    }
}