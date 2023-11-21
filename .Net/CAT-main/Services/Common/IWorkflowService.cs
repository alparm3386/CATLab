using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IWorkflowService
    {
        Task CreateWorkflowAsync(int orderId);

        Task<WorkflowStep> GetCurrentStepAsync(int jobId);

        Task<WorkflowStep> GetNextStepAsync(int jobId);

        Task StartNextStepAsync(int jobId);

        Task StartWorkflowAsync(int jobId);
        Task StepBackAsync(int jobId);
    }
}