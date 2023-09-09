using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CAT.Models.Entities.Main
{
    public class LinguistLanguagePair
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SourceLanguage { get; set; }

        public int TargetLanguage { get; set; }

        public int Speciality { get; set; }

        public int Roles { get; set; }

        public String Comments { get; set; } = default!;
    }
}
