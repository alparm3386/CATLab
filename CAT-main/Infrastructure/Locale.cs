using CAT.Models.Entities.Main;
using System.Globalization;

namespace CAT.Infrastructure
{
    public class LocaleId
    {
        //private CultureInfo _cultureInfo;
        private string _languageISO639_1;

        public LocaleId(string language)
        {
            _languageISO639_1 = language;
            //if (CultureInfo.GetCultures(CultureTypes.AllCultures).Any(c => c.Name == cultureName))
            //{
            //    // If it's a standard culture
            //    _cultureInfo = new CultureInfo(cultureName);
            //}
            //else
            //{
            //    // Attempt to create and register the custom culture
            //    CreateAndRegisterCulture(cultureName, baseCultureName);
            //    _cultureInfo = new CultureInfo(cultureName);
            //}
        }

        //public string Language { get { return _languageISO639_1; } }
        public string Language => _languageISO639_1;

        private void CreateAndRegisterCulture(string cultureName, string baseCultureName)
        {
            //var builder = new CultureAndRegionInfoBuilder(cultureName, CultureAndRegionModifiers.None);
            //builder.LoadDataFromCultureInfo(new CultureInfo(baseCultureName));
            //builder.LoadDataFromRegionInfo(new RegionInfo(baseCultureName));

            //// Additional customizations can be done here, if needed

            //try
            //{
            //    builder.Register();
            //}
            //catch (UnauthorizedAccessException)
            //{
            //    // Handle the exception for cases where you don't have admin rights to register a new culture.
            //    throw new Exception("You must have administrative rights to register a new custom culture.");
            //}
        }

        //public CultureInfo CultureInfo => _cultureInfo;
    }

    // Usage:
    // var customCulture = new CustomCultureInfo("fr-xz");  // This will use "en-US" as base
    // var cultureInfoObj = customCulture.CultureInfo;     // Retrieve the actual CultureInfo object
}
