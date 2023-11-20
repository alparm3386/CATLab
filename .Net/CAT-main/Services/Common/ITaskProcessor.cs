namespace CAT.Services.Common
{
    public interface ITaskProcessor
    {
        void ProcessTaks(int jobId, CAT.Enums.Task task);
    }
}