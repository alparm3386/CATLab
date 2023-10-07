using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("DocumentFilters")]
    public class DocumentFilter
    {
        [Key]
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int FilterId { get; set; }

    }
}
