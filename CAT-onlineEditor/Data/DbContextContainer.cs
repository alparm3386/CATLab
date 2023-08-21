using CAT.Areas.Identity.Data;

namespace CAT.Data
{
    public class DbContextContainer
    {
        public IdentityDbContext IdentityContext { get; }
        public MainDbContext MainContext { get; }
        public TranslationUnitsDbContext TranslationUnitsContext { get; }

        public DbContextContainer(IdentityDbContext identityContext, MainDbContext mainContext, TranslationUnitsDbContext translationUnitsContext)
        {
            IdentityContext = identityContext;
            MainContext = mainContext;
            TranslationUnitsContext = translationUnitsContext;
        }
    }
}
