using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("TempDocuments")]
    public class TempDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DocumentType { get; set; }

        public string OriginalFileName { get; set; } = default!;

        public string FileName { get; set; } = default!;

        public string? MD5Hash { get; set; }

        public int FilterId { get; set; }
    }
}
