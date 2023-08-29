﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("TempQuotes")]
    public class TempQuote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int StoredQuoteId { get; set; }

        public DateTime DateCreated { get; set; }

        public int TempDocumentId { get; set; }

        public string SourceLanguage { get; set; } = default!;

        public string TargetLanguage { get; set; } = default!;

        public int SpecialityId { get; set; }

        public int Service { get; set; }

        public double Fee { get; set; }

        public int Speed { get; set; }

        public string? Analysis { get; set; } = default!;
    }
}
