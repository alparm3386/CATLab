namespace CAT.Models.Entities.Main
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Filters")]
    public class Filter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProfileId { get; set; }

        public string FilterName { get; set; } = default!;
    }
}
