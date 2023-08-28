using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CAT.Models.ViewModels
{
    public class CreateQuoteViewModel
    {
        public int StoredQuoteId { get; set; }

        [Required]
        public string? SourceLanguage { get; set; }

        [Required]
        public List<String>? TargetLanguages { get; set; }

        [Required]
        public int Speciality { get; set; }
        [Required]

        public int Filter { get; set; }

        public IFormFile? FileToUpload { get; set; }

        public int ServiceSpeed { get; set; }

        public Dictionary<string, string> Languages => new Dictionary<string, string>
        {
            {"nl", "Dutch"},
            {"de", "German"},
            {"en", "English"},
            {"fr", "French"},
            {"it", "Itaian"},
            {"es", "Spanish"}
            // Add more languages as needed
        };

        public Dictionary<int, string> Specialities => new Dictionary<int, string>
        {
            {1, "General"},
            {2, "Marketing"},
            {3, "Technical"}
        };
    }
}
