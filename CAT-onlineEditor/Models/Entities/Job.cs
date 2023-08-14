using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CATWeb.Models.Entities
{
    [Table("Jobs")]
    public class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int IdClient { get; set; }

        [Display(Name = "Original file name")]
        public string? OriginalFileName { get; set; }
        [Display(Name = "File name")]
        public string? FileName { get; set; }

        public string? FilterName { get; set; }

        [Required(ErrorMessage = "Please enter the source language.")]
        public string SourceLang { get; set; }

        [Required(ErrorMessage = "Please enter the target language.")]
        public string TargetLang { get; set; }

        [Display(Name = "Date created")]
        [DataType(DataType.Date)]

        public DateTime DateCreated { get; set; }

        public string? Analysis { get; set; }

        public decimal Fee { get; set; }

        public DateTime? DateProcessed { get; set; }
    }
}
