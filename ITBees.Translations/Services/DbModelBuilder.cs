using ITBees.Translations.SqlMigration;
using Microsoft.EntityFrameworkCore;

namespace ITBees.Translations.Services
{
    public class DbModelBuilder
    {
        public static void Register(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RuntimeTranslation>().HasIndex(x => new {x.BasePhraseId, x.LanguageId}).IsUnique();
            modelBuilder.Entity<BasePhrase>().HasIndex(x => x.Phrase).IsUnique();
        }
    }
}