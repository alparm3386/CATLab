using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Orders")]
    public class Order
    {
        public Order()
        {
            Jobs = new List<Job>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ClientId { get; set; }

        public DateTime DateCreated { get; set; }

        public ICollection<Job> Jobs { get; set; }

    }
}
