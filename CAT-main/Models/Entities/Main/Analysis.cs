using CAT.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace CAT.Models.Entities.Main
{
    [Table("Analysis")]
    public class Analysis
    {
        public int DocumentId { get; set; }

        public AnalysisType Type { get; set; }

        [MaxLength(10)]
        public string? SourceLanguage { get; set; }

        [MaxLength(10)]
        public string? TargetLanguage { get; set; }

        public int Speciality { get; set; }

        public int Repetitions { get; set; }

        public int Match_100 { get; set; }
        public int Match_101 { get; set; }
        public int Match_50_74 { get; set; }
        public int Match_75_84 { get; set; }
        public int Match_85_94 { get; set; }
        public int Match_95_99 { get; set; }
        public int No_match { get; set; }

    }
}
