using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("TempQuotes")]
    public class TempQuote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int StoredQuoteId { get; set; }

        public DateTime DateCreated { get; set; }

        public int TempDocumentId { get; set; }
        public TempDocument TempDocument { get; set; } = default!;

        [Display(Name = "Source language")]
        public int SourceLanguage { get; set; }

        [Display(Name = "Target language")]
        public int TargetLanguage { get; set; }

        public int SpecialityId { get; set; }

        public int Service { get; set; }

        public bool ClientReview { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public double Fee { get; set; }

        public int Speed { get; set; }

        public string? Analysis { get; set; } = default!;
    }
}
