using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace CAT.Models.Entities.TranslationUnits
{
    [Index(nameof(documentId))]
    [Table("TranslationUnits")]
    public class TranslationUnit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int documentId { get; set; }

        public int tuid { get; set; }

        public string? source { get; set; }

        public string? context { get; set; }

        public int locks { get; set; }

        public string? target { get; set; }

        public DateTime dateUpdated { get; set; }

        public long status { get; set; }
    }
}