using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Addresses")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]  
        public string Line1 { get; set; } = default!;

        [StringLength(200)] 
        public string? Line2 { get; set; } 

        [Required]
        [StringLength(100)]  
        public string City { get; set; } = default!;


        [Required]
        [StringLength(10)] 
        public string PostalCode { get; set; } = default!;

        [Required]
        [StringLength(50)] 
        public string Country { get; set; } = default!;

        // Optional fields
        [StringLength(50)] 
        public string? Region { get; set; }  // Region or Territory, can be optional

        [StringLength(15)]
        public string? Phone { get; set; }  // In case you want to associate a phone number with the address
    }
}
