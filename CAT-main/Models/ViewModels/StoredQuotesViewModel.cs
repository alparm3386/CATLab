using CAT.Enums;
using CAT.Models.Entities.Main;

namespace CAT.Models.ViewModels
{
    public class StoredQuotesViewModel
    {
        public StoredQuotesViewModel()
        {
        }

        public List<StoredQuote> StoredQuotes { get; set; } = default!;
    }
}
