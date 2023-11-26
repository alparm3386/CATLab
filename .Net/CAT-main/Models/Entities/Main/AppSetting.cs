using CAT.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace CAT.Models.Entities.Main
{
    [Table("AppSettings")]
    public class AppSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Key { get; set; } = default!;

        [MaxLength(512)]
        public string Value { get; set; } = default!;
    }
}
