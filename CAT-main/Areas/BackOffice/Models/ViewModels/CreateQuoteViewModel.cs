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

        [Required(ErrorMessage = "Please upload a file.")]
        public IFormFile? FileToUpload { get; set; }

        public int ServiceSpeed { get; set; }

        public bool ClientReview { get; set; }
    }
}
