using CAT.Models.Common;

namespace CAT.Models
{
    public class JobData
    {
        public int jobId;
        public List<TranslationUnitDTO> translationUnits = default!;
        public List<TMAssignment> tmAssignments = default!;
        public List<TBAssignment> tbAssignments = default!;
        public int task;
        public object user = default!;
        public object pmUser = default!;
    }
}
