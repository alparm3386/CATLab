namespace CAT.Models.Common
{
    public class TMMatch
    {
        public int id { get; set; }
        public string? source { get; set; }
        public string? target { get; set; }
        public string? origin { get; set; }
        public int quality { get; set; }
        public string? metadata { get; set; }
    }
}
