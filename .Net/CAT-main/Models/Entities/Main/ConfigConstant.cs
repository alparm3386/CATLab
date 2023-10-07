using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CAT.Models.Entities.Main
{
    [Table("ConfigConstants")]
    public class ConfigConstant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Key { get; set; } = default!;

        public string Value { get; set; } = default!;
    }
}
