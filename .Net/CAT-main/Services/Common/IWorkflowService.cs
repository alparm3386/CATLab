namespace CAT.Services.Common
{
    public interface IWorkflowService
    {
        public Task CreateWorkflowAsync(int orderId);
        void StartNextStep(int jobId);
    }
}