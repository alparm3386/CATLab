namespace CAT.Services.Common
{
    public class TaskProcessor : ITaskProcessor
    {
        private readonly ICATConnector _catconnector;

        public TaskProcessor(ICATConnector catconnector)
        {
            _catconnector = catconnector;
        }

        public void ProcessTaks(int jobId, CAT.Enums.Task task)
        {
        }
    }
}
