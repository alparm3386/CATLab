using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Comapnies")]
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CompanyGroupId { get; set; }

        public string Name { get; set; } = default!;

        public int AddressId { get; set; }

        public Address Address { get; set; } = default!;
    }
}
