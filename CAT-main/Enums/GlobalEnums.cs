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
        Unspecified = -1,
        Original,
        Translated
    }

    public enum Task
    {
        Unknown = -1,
        Jobboard = 1,
        AIProcess = 2,
        Translation = 3,
        Revision = 4,
        ClientReview = 5,
        End = 100
    }

    public enum AnalysisType
    {
        Normal,
        WithGlobalTM
    }
}
