namespace CAT.Services.Common
{
    public interface IWorkflowService
    {
        System.Threading.Tasks.Task CreateWorkflowAsync(int orderId);
        System.Threading.Tasks.Task StartNextStepAsync(int jobId);
        Task StartWorkflowAsync(int jobId);
    }
}