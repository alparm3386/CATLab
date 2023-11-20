using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("BackgroundProcesses")]
    public class BackgroundProcess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(50)]
        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public string? ProcessId { get; set; }

        [MaxLength(200)]
        public string TaskName { get; set; } = default!;

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public DateTime ProcessStarted { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public DateTime? ProcessEnded { get; set; }
    }
}
