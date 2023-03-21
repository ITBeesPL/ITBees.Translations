using ITBees.Translations.Interfaces;
using System.Transactions;

namespace ITBees.Translations.Translations
{
    public class TranslateMessages : ITranslate
    {
        public static readonly string YouMustLoadTranslationFilesFirst =
            "You must load translation files firs, use method : Translate.LoadFiles(path);";

        public static readonly string InvalidExpression = "Invalid expression";
    }
}