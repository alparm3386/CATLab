namespace CAT.Services.Common
{
    public interface IMonitoringService
    {
        Task<object> GetJobData(int jobId);
        Task<dynamic> GetMonitoringData(DateTime dateFrom, DateTime dateTo);
    }
}