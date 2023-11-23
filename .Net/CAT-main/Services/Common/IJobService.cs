using CAT.Models.Common;

namespace CAT.Services.Common
{
    public interface IJobService
    {
        FileData CreateDocument(int jobId, string userId, bool updateTM);

        Task SubmitJobAsync(int jobId, string userId);
    }
}