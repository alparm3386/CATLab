using CAT.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CATWeb.Models.Entities
{
    [Table("Jobs")]
    public class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int OrderId { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public Order Order { get; set; }

        public int QuoteId { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public Quote Quote { get; set; }

        public int DocumentId { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public Document Document { get; set; }

        public ICollection<WorkflowStep> WorkflowSteps { get; set; }
    }
}
