using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models.Entities.Main
{
    [Table("WorkflowSteps")]
    public class WorkflowStep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int JobId { get; set; }

        public int StepOrder { get; set; }

        public int TaskId { get; set; }

        public int Status { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime CompletionDate { get; set; }

        public decimal? Fee { get; set; }
    }
}
