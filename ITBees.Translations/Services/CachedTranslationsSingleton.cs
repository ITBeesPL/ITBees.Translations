using System.Collections.Concurrent;
using System.Linq;
using ITBees.Interfaces.Repository;
using ITBees.Translations.Interfaces;
using ITBees.Translations.SqlMigration;

namespace ITBees.Translations.Services;

public class CachedTranslationsSingleton : ICachedTranslationsSingleton
{
    private readonly ConcurrentDictionary<string, CachedTranslation> _translations;

    public CachedTranslationsSingleton(IReadOnlyRepository<RuntimeTranslation> roRepoRuntimeTranslation)
    {
        var translations = roRepoRuntimeTranslation.GetData(x=>true)
            .Select(rt => new
            {
                Key = $"{rt.BasePhrase.Phrase}_{rt.LanguageId}",
                Translation = new CachedTranslation
                {
                    Found = true,
                    Value = rt.TanslationValue,
                    HasReplicableFields = rt.HasReplicableFields,
                    ReplicableFields = rt.ReplicableFields
                }
            })
            .ToDictionary(x => x.Key, x => x.Translation);

        _translations = new ConcurrentDictionary<string, CachedTranslation>(translations);
    }

    public CachedTranslation GetTranslation(string key, int languageId)
    {
        var cacheKey = $"{key}_{languageId}";

        if (_translations.TryGetValue(cacheKey, out var translation))
        {
            return translation;
        }

        return new CachedTranslation { Found = false };
    }

    public void AddTranslation(string key, int languageId, string translationValue, bool hasReplicableFields = false, string replicableFields = null)
    {
        var cacheKey = $"{key}_{languageId}";

        var newTranslation = new CachedTranslation
        {
            Found = true,
            Value = translationValue,
            HasReplicableFields = hasReplicableFields,
            ReplicableFields = replicableFields
        };

        _translations[cacheKey] = newTranslation;
    }
}