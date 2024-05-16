using System.ComponentModel.DataAnnotations;
using ITBees.Models.Languages;

namespace ITBees.Translations.SqlMigration
{
    public class RuntimeTranslation
    {
        [Key]
        public int Id { get; set; }
        public string TranslationKey { get; set; }
        public string TanslationValue { get; set; }
        public Language Language { get; set; }
        public int LanguageId { get; set; }
        public bool HasReplicableFields { get; set; }
        public string ReplicableFields { get; set; }
    }
}