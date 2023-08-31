using CAT.Enums;
using CAT.Helpers;
using System;
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

        [Required]
        public int Service { get; set; }

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

        public Dictionary<int, string> Specialities => Enum.GetValues(typeof(Speciality))
                                           .Cast<Speciality>()
                                           .ToDictionary(e => (int)e, e => e.ToString());

        public Dictionary<int, string> Services => Enum.GetValues(typeof(Service))
                                                   .Cast<Service>()
                                                   .ToDictionary(e => (int)e, e => e.GetDisplayName());

        public Dictionary<int, string> Filters
        {
            get
            {
                var filters = new Dictionary<int, string>()
                {
                    {-1, "No filter"},
                    {1, "Excel 3 column"},
                    {2, "Word yellow highlighted"}
                };

                return filters;
            }
            set
            {
            }
        }
    }
}
