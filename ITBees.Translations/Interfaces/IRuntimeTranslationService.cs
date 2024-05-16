using System.Collections.Generic;
using ITBees.Models.Languages;

namespace ITBees.Translations.Interfaces
{
    public interface IRuntimeTranslationService
    {
        string GetTranslation(string key, Language lang, List<ReplaceableValue> replaceableValues = null);
    }
}

public class ReplaceableValue
{
    public string FieldName { get; set; }
    public string FieldValue { get; set; }
}