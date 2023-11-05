using CAT.Models.Common;

namespace CAT.Services.Common
{
    public interface ICATConnector
    {
        bool CanBeParsed(string sFilePath);
        FileData CreateDoc(int idJob, string userId, bool updateTM);
        void CreateXliffFromDocument(string sTranslationDir, string sOutFileName, string sFilePath, string sFilterPath, string sourceLang, string targetLang, int iGoodMatchRate, TMAssignment[] tmAssignments);
        Task<Statistics[]> GetStatisticsForDocument(string sFilePath, string sFilterPath, int sourceLang, int[] aTargetLangs, TMAssignment[] tmAssignments);
        void ParseDoc(int idJob);
    }
}