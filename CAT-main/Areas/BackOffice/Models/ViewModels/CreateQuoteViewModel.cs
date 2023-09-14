using CAT.Enums;
using CAT.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CAT.Areas.BackOffice.Models.ViewModels
{
    public class CreateQuoteViewModel
    {
        //[Required]
        //public int ClientId { get; set; }

        public int StoredQuoteId { get; set; }

        [Required]
        public int SourceLanguage { get; set; }

        [Required]
        public List<int>? TargetLanguages { get; set; }

        [Required]
        public int Speciality { get; set; }

        [Required]
        public int Filter { get; set; }

        [Required]
        public int Service { get; set; }

        public IFormFile? FileToUpload { get; set; }

        public int ServiceSpeed { get; set; }

        public bool ClientReview { get; set; }

        public Dictionary<int, string> Languages => new Dictionary<int, string>
        {
            {1, "English"},
            {2, "French"},
            {3, "German"},
            {4, "Itaian"},
            {5, "Dutch"},
            {6, "Spanish"}
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
