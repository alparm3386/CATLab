using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Clients")]
    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CompanyId { get; set; }

        public Company Company { get; set; } = default!;

        public string UserId { get; set; } = default!;

        public int AddressId { get; set; }

        public Address Address { get; set; } = default!;

        //other client specific fields comes here...
    }
}
