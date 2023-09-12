using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Rates")]
    public class Rate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SourceLanguageId { get; set; }

        public int TargetLanguageId { get; set; }

        public int Speciality { get; set; }

        public int Task { get; set; }

        public float RateToClient { get; set; }

        public float RateToTranslator { get; set; }
    }
}
