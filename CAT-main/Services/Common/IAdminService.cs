namespace CAT.Services.Common
{
    public interface IAdminService
    {
        void AllocateJob(int jobId, int linguistId, Task task);
    }
}
