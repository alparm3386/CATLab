using System.ComponentModel.DataAnnotations;

namespace CAT_web.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string? OriginalFileName { get; set; }
        public string? FileName { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }
        public string? Analysis { get; set; }
        public decimal Price { get; set; }
    }
}
