using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("ClientRates")]
    public class ClientRate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; } = default!;

        public int RateId { get; set; }

        public Rate Rate { get; set; } = default!;

        public int Currency { get; set; }

        public float RateToClient { get; set; }
    }
}
