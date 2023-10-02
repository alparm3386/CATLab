namespace CAT.Areas.BackOffice.Services
{
    public interface IMonitoringService
    {
        Task AllocateJob(int jobId, Enums.Task task, string userId);
        Task DeallocateJob(int jobId, Enums.Task task, string comment);
        Task<object> GetJobData(int jobId);
        Task<dynamic> GetMonitoringData(DateTime dateFrom, DateTime dateTo);
    }
}