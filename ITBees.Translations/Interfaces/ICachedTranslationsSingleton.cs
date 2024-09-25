namespace ITBees.Translations.Interfaces;

public interface ICachedTranslationsSingleton
{
    CachedTranslation GetTranslation(string key, int languageId);
    void AddTranslation(string key, int languageId, string translationValue, bool hasReplicableFields = false, string replicableFields = null);
}