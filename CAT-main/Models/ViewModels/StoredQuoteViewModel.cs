using CAT.Models.Entities.Main;

namespace CAT.Models.ViewModels
{
    public class StoredQuoteViewModel
    {
        public StoredQuote StoredQuote { get; set; } = default!;

        public double Fee 
        {
            get 
            {
                if (StoredQuote != null)
                {

                }

                return 0;
            }
        }
    }
}
