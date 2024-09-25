using System;
using System.Collections.Concurrent;
using System.Linq;
using ITBees.Interfaces.Repository;
using ITBees.Translations.Interfaces;
using ITBees.Translations.SqlMigration;
using Microsoft.Extensions.DependencyInjection;

namespace ITBees.Translations.Services
{
    public class CachedTranslationsSingleton : ICachedTranslationsSingleton
    {
        private readonly ConcurrentDictionary<string, CachedTranslation> _translations = new ConcurrentDictionary<string, CachedTranslation>();
        private readonly IServiceProvider _serviceProvider;
        private bool _isInitialized = false;
        private readonly object _initLock = new object();

        public CachedTranslationsSingleton(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;

            lock (_initLock)
            {
                if (_isInitialized)
                    return;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var roRepoRuntimeTranslation = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<RuntimeTranslation>>();
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

                    foreach (var kvp in translations)
                    {
                        _translations[kvp.Key] = kvp.Value;
                    }
                }

                _isInitialized = true;
            }
        }

        public CachedTranslation GetTranslation(string key, int languageId)
        {
            Initialize();

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
}
