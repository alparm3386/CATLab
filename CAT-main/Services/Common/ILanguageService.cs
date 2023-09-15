using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface ILanguageService
    {
        String GetLanguageCodeIso639_1(int languageId);
        int GetLanguageIdFromIso639_1Code(string laguageCode);
        Task<Dictionary<int, Language>> GetLanguages();
    }
}
