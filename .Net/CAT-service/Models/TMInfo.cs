using CAT.Enums;

namespace CAT.Models
{
    public class TMInfo
    {
        public String id = default!;
        public String langFrom = default!;
        public String langTo = default!;
        public int entryNumber;
        public TMType tmType;
        public DateTime lastAccess;
    }
}


public class TestResponse
{
    public long LastAccess { get; set; }
    public int TmType { get; set; }
    // ... other properties
}
