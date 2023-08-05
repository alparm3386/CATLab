using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace CATWeb.Models
{
    [Index(nameof(idJob))]
    [Table("TranslationUnits")]
    public class TranslationUnit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; set; }

        public int idJob { get; set; }

        public int tuid { get; set; }

        public string? source { get; set; }

        public string? context { get; set; }

        public int locks { get; set; }

        public string? target { get; set; }

        public DateTime dateUpdated { get; set; }

        public long status { get; set; }
    }
}