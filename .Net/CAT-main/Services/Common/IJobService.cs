using CAT.Models.Common;

namespace CAT.Services.Common
{
    public interface IJobService
    {
        FileData CreateDocument(int idJob, string userId, bool updateTM);
    }
}