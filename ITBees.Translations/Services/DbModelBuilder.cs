using ITBees.Translations.SqlMigration;
using Microsoft.EntityFrameworkCore;

namespace ITBees.Translations.Services
{
    public class DbModelBuilder
    {
        public static void Register(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RuntimeTranslation>().HasKey(x => x.Id);
        }
    }
}