namespace CAT.Areas.BackOffice.Services
{
    public interface IMonitoringService
    {
        Task AllocateJob(int jobId, Enums.Task task, string userId, string allocatorUserId);
        Task DeallocateJob(int jobId, string userId, Enums.Task task, string deallocationReason);
        Task<object> GetJobData(int jobId);
        Task<dynamic> GetMonitoringData(DateTime dateFrom, DateTime dateTo);
    }
}