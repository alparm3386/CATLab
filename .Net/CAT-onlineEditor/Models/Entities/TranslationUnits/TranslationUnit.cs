using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace CAT.Models.Entities.TranslationUnits
{
    [Index(nameof(DocumentId))]
    [Table("TranslationUnits")]
    public class TranslationUnit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DocumentId { get; set; }

        public int Tuid { get; set; }

        public string? Source { get; set; }

        public string? Context { get; set; }

        public int Locks { get; set; }

        public string? Target { get; set; }

        public DateTime DateUpdated { get; set; }

        public long Status { get; set; }
    }
}