using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT_web.Models
{
    [Table("Jobs")]
    public class Job
    {
        public int Id { get; set; }
        [Display(Name = "Original file name")]
        public string? OriginalFileName { get; set; }
        [Display(Name = "File name")]
        public string? FileName { get; set; }

        [Display(Name = "Date created")]
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }
        public string? Analysis { get; set; }
        public decimal Price { get; set; }
    }
}
