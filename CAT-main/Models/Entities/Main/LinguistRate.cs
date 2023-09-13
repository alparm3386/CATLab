using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("LinguistRates")]
    public class LinguistRate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int LinguistId { get; set; }
        public Linguist Linguist { get; set; } = default!;

        public int RateId { get; set; }

        public Rate Rate { get; set; } = default!;

        public int Currency { get; set; }

        public float RateToLinguist { get; set; }
    }
}
