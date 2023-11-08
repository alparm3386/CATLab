using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CAT.Models.Entities.Main
{
    [Table("StoredQuotes")]
    public class StoredQuote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime? DateCreated { get; set; }

        public int ClientId { get; set; }

        public Client Client { get; set; } = default!;

        public int? OrderId { get; set; }

        public ICollection<TempQuote> TempQuotes { get; set; } = default!;

        public double Fee
        {
            get
            {
                if (TempQuotes != null)
                {
                    return TempQuotes.Sum(tq => tq.Fee);
                }

                return 0;
            }
        }
    }
}
