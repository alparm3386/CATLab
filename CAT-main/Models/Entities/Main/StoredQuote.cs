using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CAT.Models.Entities.Main
{
    public class StoredQuote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime DateCreated { get; set; }

        public int ClientId { get; set; }

        public int OrderId { get; set; }
    }
}
