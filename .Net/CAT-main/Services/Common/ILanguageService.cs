using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface ILanguageService
    {
        Task<string> GetLanguageCodeIso639_1Async(int languageId);
        Task<int> GetLanguageIdFromIso639_1CodeAsync(string laguageCode);
        Task<Dictionary<int, Language>> GetLanguagesAsync();
    }
}
