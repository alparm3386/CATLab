namespace CAT.Models.ViewModels
{
    public class QuoteCalculatorViewModel
    {
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
        public int Speciality { get; set; }
        public IFormFile? FileToUpload { get; set; }

        public Dictionary<string, string> Languages => new Dictionary<string, string>
        {
            {"en", "English"},
            {"fr", "French"},
            {"de", "German"},
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
