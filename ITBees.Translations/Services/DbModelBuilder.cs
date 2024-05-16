using ITBees.Translations.SqlMigration;

namespace ITBees.Translations.Services
{
    public class DbModelBuilder
    {
        public static void Register(dynamic modelBuilder)
        {
            modelBuilder.Entity<RuntimeTranslation>();
        }
    }
}