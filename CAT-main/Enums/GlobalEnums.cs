using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CAT.Enums
{
    public enum OEMode
    {
        Admin,
        Linguist,
        Client
    }

    public enum DocumentType
    {

        [Display(Name = "Unknown task")]
        Unspecified = -1,
        [Display(Name = "Original")]
        Original = 0,
        [Display(Name = "Processed by AI")]
        AI = 1,
        [Display(Name = "Translated")]
        Translated = 2,
        [Display(Name = "Revised")]
        Revised = 3,
        [Display(Name = "Rectified")]
        Rectified = 4,
        [Display(Name = "Reviewed by client")]
        ReviewedByClient = 5
    }

    public enum Task
    {
        [Display(Name = "Unknown task")]
        Unknown = -1,
        [Display(Name = "Job board")]
        Jobboard = 1,
        [Display(Name = "AI process")]
        AIProcess = 2,
        [Display(Name = "Translation")]
        Translation = 3,
        [Display(Name = "Revision")]
        Revision = 4,
        [Display(Name = "Client review")]
        ClientReview = 5,
        [Display(Name = "Job completed")]
        Completed = 6,
        [Display(Name = "Credit")]
        CreditLinguists = 7,
        [Display(Name = "Billing")]
        Billing = 8,
        [Display(Name = "Delivery")]
        Delivery = 9,
        [Display(Name = "End")]
        End = 100
    }

    public enum WorkflowStatus
    {
        [Display(Name = "Not started")]
        NotStarted = 0,
        [Display(Name = "In progress")]
        InProgress = 1,
        [Display(Name = "Completed")]
        Completed = 2,
        [Display(Name = "Cancelled")]
        Cancelled = 3
    }

    public enum AnalysisType
    {
        Normal,
        WithGlobalTM
    }

    public enum ServiceType
    {
        Unknown = -1,
        AI = 1,
        AIWithRevision = 2
    }

    public enum ServiceSpeed
    {
        [Display(Name = "Regular Speed")]
        Normal,

        [Display(Name = "Express Speed")]
        Express
    }

    public enum Speciality
    {
        [Display(Name = "General")]
        General = 1,
        [Display(Name = "Marketing")]
        Marketing = 2,
        [Display(Name = "Technical")]
        Technical = 3
    }

    public enum Service
    {
        [Display(Name = "AI")]
        AI = 1,
        [Display(Name = "AI with human revision")]
        AIWithHumanRevision
    }
}
