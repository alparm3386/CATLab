using CATWeb.Models.CAT;

namespace CATWeb.Models
{
    public class JobData
    {
        public int idJob;
        public List<TranslationUnit>? translationUnits;
        public List<TMAssignment>? tmAssignments;
        public List<TBAssignment>? tbAssignments;
    }
}
