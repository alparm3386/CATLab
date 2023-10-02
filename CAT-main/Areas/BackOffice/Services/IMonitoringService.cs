namespace CAT.Areas.BackOffice.Services
{
    public interface IMonitoringService
    {
        Task AllocatJob(int jobId, Enums.Task task, string userId);
        Task<object> GetJobData(int jobId);
        Task<dynamic> GetMonitoringData(DateTime dateFrom, DateTime dateTo);
    }
}