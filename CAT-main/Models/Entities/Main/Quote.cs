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

        public double Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public string SourceLanguage { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public string TargetLanguage { get; set; }

        public int Speciality { get; set; }
    }
}
