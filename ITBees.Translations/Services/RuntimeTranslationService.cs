using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITBees.Interfaces.Repository;
using ITBees.Models.Languages;
using ITBees.Translations.Interfaces;
using ITBees.Translations.SqlMigration;

namespace ITBees.Translations.Services
{
    public class RuntimeTranslationService : IRuntimeTranslationService
    {
        private readonly IReadOnlyRepository<BasePhrase> _roBasePhrase;
        private readonly IWriteOnlyRepository<BasePhrase> _rwBasePhrase;
        private readonly IWriteOnlyRepository<RuntimeTranslation> _rwRepoRuntimeTranslation;
        private readonly IChatGptConnector _gptConnector;
        private readonly ICachedTranslationsSingleton _cachedTranslations;

        public RuntimeTranslationService(
            IReadOnlyRepository<BasePhrase> roBasePhrase,
            IWriteOnlyRepository<BasePhrase> rwBasePhrase,
            IWriteOnlyRepository<RuntimeTranslation> rwRepoRuntimeTranslation,
            IChatGptConnector gptConnector,
            ICachedTranslationsSingleton cachedTranslations)
        {
            _roBasePhrase = roBasePhrase;
            _rwBasePhrase = rwBasePhrase;
            _rwRepoRuntimeTranslation = rwRepoRuntimeTranslation;
            _gptConnector = gptConnector;
            _cachedTranslations = cachedTranslations;
        }

        public async Task<string> GetTranslation(string key, Language lang, bool askChatGptForTranslationIfMissing,
            List<ReplaceableValue> replaceableValues = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var cachedTranslation = _cachedTranslations.GetTranslation(key, lang.Id);

            if (cachedTranslation.Found)
            {
                var result = cachedTranslation;

                if (result.HasReplicableFields)
                {
                    if (replaceableValues == null)
                    {
                        throw new Exception($"Translation for Key :{key} requires replaceable values: {result.ReplicableFields}");
                    }

                    var targetTranslationResult = result.Value;
                    var fieldsToBeReplaced = result.ReplicableFields.Split(';');
                    foreach (var field in fieldsToBeReplaced)
                    {
                        var replacedValue = replaceableValues.FirstOrDefault(x => x.FieldName == field);
                        if (replacedValue == null)
                        {
                            throw new Exception($"Missing replaceable value for field '{field}' in key '{key}'");
                        }
                        targetTranslationResult = targetTranslationResult.Replace($"[[{field}]]", replacedValue.FieldValue);
                    }

                    return targetTranslationResult;
                }
                else
                {
                    return result.Value;
                }
            }
            else
            {
                if (!askChatGptForTranslationIfMissing)
                    throw new Exception($"No translation for key: {key} and language: {lang.Code}");

                var chatResult = await _gptConnector.AskChatGptAsync(
                    $"Provide me with the translation into the language: {lang.Name} of this phrase: '{key}', return the answer as a string only, without additional comments, without characters, and without quotation marks");

                chatResult = RemoveQuotes(chatResult); //model gpt-4 still ignores this direct command and returns with quotations some results

                var basePhrase = _roBasePhrase.GetData(x => x.Phrase == key).FirstOrDefault();

                if (basePhrase == null)
                {
                    basePhrase = _rwBasePhrase.InsertData(new BasePhrase { Phrase = key });
                }

                var newTranslation = new RuntimeTranslation
                {
                    BasePhraseId = basePhrase.Id,
                    HasReplicableFields = false,
                    LanguageId = lang.Id,
                    TanslationValue = chatResult
                };

                _rwRepoRuntimeTranslation.InsertData(newTranslation);

                _cachedTranslations.AddTranslation(key, lang.Id, chatResult);

                return chatResult;
            }
        }
        public static string RemoveQuotes(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Sprawdzamy czy pierwszy i ostatni znak to "
            if (input.StartsWith("\"") && input.EndsWith("\""))
            {
                // Usuwamy pierwszy i ostatni znak
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }
    }
}