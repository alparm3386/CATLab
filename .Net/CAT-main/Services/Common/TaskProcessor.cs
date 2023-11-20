using CAT.Models.Entities.Main;
using CAT.Enums;
using Task = CAT.Enums.Task;

namespace CAT.Services.Common
{
    public class TaskProcessor : ITaskProcessor
    {
        private readonly ICATConnector _catconnector;

        public TaskProcessor(ICATConnector catconnector)
        {
            _catconnector = catconnector;
        }

        public bool ProcessTask(WorkflowStep workflowStep)
        {
            //parse and pre-translate the document
            if (workflowStep.TaskId == (int)Task.NewJob)
            {
                _catconnector.ParseDoc(workflowStep.JobId);
                return true;
            }

            return false;
        }
    }
}
