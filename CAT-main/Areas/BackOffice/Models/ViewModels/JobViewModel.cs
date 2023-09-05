using System;
using System.ComponentModel;

namespace CAT.Areas.BackOffice.Models.ViewModels
{
    public class JobViewModel
    {
        public int Id { get; set; }

        [DisplayName("Original File Name")]
        public string? OriginalFileName { get; set; }

        [DisplayName("Date Created")]
        public DateTime? DateCreated { get; set; }

        [DisplayName("Date Processed")]
        public DateTime? DateProcessed { get; set; }

        [DisplayName("Analysis")]
        public string? Analysis { get; set; }

        [DisplayName("Fee (£)")]
        public double Fee { get; set; }

        [DisplayName("Speed")]
        public int Speed { get; set; }
    }
}
