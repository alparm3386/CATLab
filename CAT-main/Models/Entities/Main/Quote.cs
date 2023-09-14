using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Quotes")]
    public class Quote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime DateCreated { get; set; }

        public int SourceLanguage { get; set; } = default!;

        public int TargetLanguage { get; set; } = default!;

        public int Speciality { get; set; }

        public int Service { get; set; }

        public bool ClientReview { get; set; }

        public int Words { get; set; }

        public double Fee { get; set; }

        public int Speed { get; set; }
    }
}
