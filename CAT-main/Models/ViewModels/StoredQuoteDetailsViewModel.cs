using CAT.Enums;
using CAT.Models.Entities.Main;

namespace CAT.Models.ViewModels
{
    public class StoredQuoteDetailsViewModel
    {
        public StoredQuoteDetailsViewModel()
        {
            Specialities = Enum.GetValues(typeof(Speciality))
                                           .Cast<Speciality>()
                                           .ToDictionary(e => (int)e, e => e.ToString());
        }

        public StoredQuote StoredQuote { get; set; } = default!;
        public Dictionary<int, string> Specialities { get; set; } = default!;
    }
}
