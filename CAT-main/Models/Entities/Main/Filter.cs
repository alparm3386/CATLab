namespace CAT.Models.Entities.Main
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace CAT.Models.Entities.Main
    {
        [Table("Filters")]
        public class Filter
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            public int ProfileId;

            public int FilterName;
        }
    }
}
