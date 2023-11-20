using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface ITaskProcessor
    {
        bool ProcessTask(WorkflowStep workflowStep);
    }
}