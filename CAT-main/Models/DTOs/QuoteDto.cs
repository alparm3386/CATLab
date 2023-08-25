using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CAT.Models.DTOs
{
    public class QuoteDto
    {
        public int Id { get; set; }

        public DateTime DateCreated { get; set; }

        public string? SourceLanguage { get; set; }

        public string? TargetLanguage { get; set; }

        public int Speciality { get; set; }

        public int Service { get; set; }

        public double Fee { get; set; }
    }
}
