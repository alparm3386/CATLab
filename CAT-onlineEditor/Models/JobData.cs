using CAT.Models.CAT;

namespace CAT.Models
{
    public class JobData
    {
        public int idJob;
        public List<TranslationUnitDTO>? translationUnits;
        public List<TMAssignment>? tmAssignments;
        public List<TBAssignment>? tbAssignments;
    }
}
