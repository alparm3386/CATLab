using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ClientId { get; set; }

        public Client Client { get; set; } = default!;

        public DateTime DateCreated { get; set; }

        public ICollection<Job> Jobs { get; set; } = default!;

        public double Fee
        {
            get
            {
                if (Jobs != null)
                {
                    return Jobs.Sum(j => j.Quote!.Fee);
                }

                return 0;
            }
        }

        public double Words
        {
            get
            {
                if (Jobs != null)
                {
                    return Jobs.Sum(j => j.Quote!.Words);
                }

                return 0;
            }
        }
    }
}
