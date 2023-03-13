using ITBees.BaseServices.Platforms.Interfaces;
using ITBees.Models.Languages;

namespace ITBees.Translations
{
    public abstract class Translator : ITranslator
    {
        public string Get(Language language, string key)
        {
            return key;
        }

        public string Get(Language language, LangConst langConst)
        {
            return langConst.Value;
        }
    }
}
