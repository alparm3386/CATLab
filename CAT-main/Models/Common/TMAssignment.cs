namespace CAT.Models.CAT
{
    public class TMAssignment
    {
        public String? tmPath;
        public int penalty;
        public int speciality = -1;
        public int penaltyForOtherSpecialities;
        public bool isReadonly = false;
        public bool isGlobal = false;
    }
}
