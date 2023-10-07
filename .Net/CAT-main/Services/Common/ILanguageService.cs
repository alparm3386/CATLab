using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface ILanguageService
    {
        Task<string> GetLanguageCodeIso639_1(int languageId);
        Task<int> GetLanguageIdFromIso639_1Code(string laguageCode);
        Task<Dictionary<int, Language>> GetLanguages();
    }
}
