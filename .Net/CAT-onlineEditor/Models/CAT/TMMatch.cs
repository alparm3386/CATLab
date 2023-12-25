namespace CAT.Models.Common
{
    public class TMMatch
    {
        public int Id { get; set; }
        public string? Source { get; set; }
        public string? Target { get; set; }
        public string? Origin { get; set; }
        public int Quality { get; set; }
        public string? Metadata { get; set; }
    }
}
