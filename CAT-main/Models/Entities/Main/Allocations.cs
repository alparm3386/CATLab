using CAT.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Allocations")]
    public class Allocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int JobId { get; set; }

        public Job Job { get; set; } = default!;

        public string UserId { get; set; } = default!;

        public ApplicationUser User { get; set; } = default!;

        public int TaskId { get; set; }

        public int Status { get; set; }

        public DateTime AllocationDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? Fee { get; set; }

        public bool ReturnUnsatisfactory { get; set; }

        public string? AdminComment { get; set; } = default!;

        public int WorkflowStepId { get; set; }
    }
}
