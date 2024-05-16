using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITBees.Models.Languages;

namespace ITBees.Translations.SqlMigration
{
    public class RuntimeTranslation
    {
        [Key]
        public int Id { get; set; }
        public BasePhrase BasePhrase { get; set; }
        public int BasePhraseId { get; set; }
        public string TanslationValue { get; set; }
        public Language Language { get; set; }
        public int LanguageId { get; set; }
        public bool HasReplicableFields { get; set; }
        public string ReplicableFields { get; set; }
    }

    public class BasePhrase
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Phrase")]
        public string Phrase { get; set; }
    }
}