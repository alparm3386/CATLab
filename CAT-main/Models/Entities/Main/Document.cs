using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Documents")]
    public class Document
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int JobId { get; set; }

        public int DocumentType { get; set; }

        public string OriginalFileName { get; set; } = default!;

        public string FileName { get; set; } = default!;

        public string? MD5Hash { get; set; }
    }
}
