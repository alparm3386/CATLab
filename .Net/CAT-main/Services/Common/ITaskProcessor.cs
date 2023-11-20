using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface ITaskProcessor
    {
        Task<bool> ProcessTaskAsync(WorkflowStep workflowStep);
    }
}