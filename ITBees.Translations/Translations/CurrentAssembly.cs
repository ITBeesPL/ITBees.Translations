using System.Collections.Generic;
using ITBees.Translations.Interfaces;

namespace ITBees.Translations.Translations
{
    public class CurrentAssembly
    {
        public static List<ITranslate> GetTranslationClasses()
        {
            return new List<ITranslate>() { new TranslateMessages() };
        }
    }
}