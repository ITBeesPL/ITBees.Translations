using System.Collections.Generic;
using System.Threading.Tasks;
using ITBees.Models.Languages;

namespace ITBees.Translations.Interfaces
{
    public interface IRuntimeTranslationService
    {
        Task<string> GetTranslation(string key, Language lang, bool askChatGptForTranslationIfMissing,
            List<ReplaceableValue> replaceableValues = null);
    }
}