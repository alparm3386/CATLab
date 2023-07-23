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

        [Required(ErrorMessage = "Please enter the source language.")]
        public string SourceLang { get; set; }

        [Required(ErrorMessage = "Please enter the target language.")]
        public string TargetLang { get; set; }

        [Display(Name = "Date created")]
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }
        public string? Analysis { get; set; }
        public decimal Fee { get; set; }
    }
}
