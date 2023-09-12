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

        public int ClientId { get; set; }

        public int SourceLanguageId { get; set; }

        public int TargetLanguageId { get; set; }

        public int Speciality { get; set; }

        public int Task { get; set; }

        public float Rate { get; set; }
    }
}
