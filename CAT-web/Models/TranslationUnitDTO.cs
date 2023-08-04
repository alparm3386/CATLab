namespace CATWeb.Models
{
    public class TranslationUnitDTO
    {
        public int Id { get; set; }

        public int idJob { get; set; }

        public int tuid { get; set; }

        public string? source { get; set; }

        public string? context { get; set; }

        public int locks { get; set; }

        public string? target { get; set; }

        public DateTime dateUpdated { get; set; }

        public int status { get; set; }

        public bool isEditAllowed { get; set; }
    }
}
