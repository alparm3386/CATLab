using System.ComponentModel;

namespace CAT.Enums
{
    public enum TMType
    {
        [Description("global")]
        global = 0,
        [Description("group")]
        groupPrimary = 1,
        [Description("group secondary")]
        groupSecondary = 2,
        [Description("profile")]
        profilePrimary = 3,
        [Description("profile secondary")]
        profileSecondary = 4,
    }
}
