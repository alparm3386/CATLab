using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("JobProcesses")]
    public class JobProcess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int JobId { get; set; }

        [MaxLength(50)]
        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public string ProcessId { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public DateTime ProcessStarted { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public DateTime ProcessEnded { get; set; }
    }
}
