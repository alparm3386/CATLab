using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("Jobs")]
    public class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int OrderId { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public virtual Order Order { get; set; }

        public int QuoteId { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public virtual Quote Quote { get; set; }

        public int SourceDocumentId { get; set; }

        public int CompletedDocumentId { get; set; }

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public ICollection<WorkflowStep> WorkflowSteps { get; set; }
    }
}
