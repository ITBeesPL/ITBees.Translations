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
        private readonly IReadOnlyRepository<RuntimeTranslation> _roRepoRuntimeTranslation;
        private readonly IReadOnlyRepository<BasePhrase> _roBasePhrase;
        private readonly IWriteOnlyRepository<BasePhrase> _rwBasePhrase;
        private readonly IWriteOnlyRepository<RuntimeTranslation> _rwRepoRuntimeTranslation;
        private readonly IChatGptConnector _gptConnector;

        public RuntimeTranslationService(IReadOnlyRepository<RuntimeTranslation> roRepoRuntimeTranslation,
            IReadOnlyRepository<BasePhrase> roBasePhrase,
            IWriteOnlyRepository<BasePhrase> rwBasePhrase,
            IWriteOnlyRepository<RuntimeTranslation> rwRepoRuntimeTranslation,
            IChatGptConnector gptConnector)
        {
            _roRepoRuntimeTranslation = roRepoRuntimeTranslation;
            _roBasePhrase = roBasePhrase;
            _rwBasePhrase = rwBasePhrase;
            _rwRepoRuntimeTranslation = rwRepoRuntimeTranslation;
            _gptConnector = gptConnector;
        }

        public async Task<string> GetTranslation(string key, Language lang, bool askChatGptForTranslationIfMissing,
            List<ReplaceableValue> replaceableValues = null)
        {
            var translation = _roRepoRuntimeTranslation.GetData(x => x.BasePhrase.Phrase == key && x.LanguageId == lang.Id).ToList();
            if (translation.Count > 1)
            {
                throw new Exception("To many translation for selected key");
            }

            if (translation.Any() == false)
            {
                if (askChatGptForTranslationIfMissing == false)
                    throw new Exception($"There is no translation for key :{key} and language :{lang.Code}");

                var chatResult = await _gptConnector.AskChatGptAsync(
                    $"Provide me with the translation into the language: {lang.Name} of this phrase: '{key}', return the answer as a string only, without additional comments, without characters, and without quotation marks");

                var basePhrase = _roBasePhrase.GetData(x => x.Phrase == key).FirstOrDefault();

                if (basePhrase == null)
                {
                    basePhrase = _rwBasePhrase.InsertData(new BasePhrase() { Phrase = key });
                }

                var savedNewTranslation = _rwRepoRuntimeTranslation.InsertData(new RuntimeTranslation()
                {
                    BasePhraseId = basePhrase.Id,
                    HasReplicableFields = false,
                    LanguageId = lang.Id,
                    TanslationValue = chatResult
                });

                translation = [savedNewTranslation];
            }

            var result = translation[0];
            if (result.HasReplicableFields)
            {
                if (replaceableValues == null)
                {
                    throw new Exception($"Translation for Key :{key} need to be given with replaceable value : {result.ReplicableFields}");
                }

                var targetTranslationResult = result.TanslationValue;
                var replacedValuesCount = 0;
                var fieldsToBeReplaced = result.ReplicableFields.Split(";");
                foreach (string s in fieldsToBeReplaced)
                {
                    var replacedValue = replaceableValues.First(x => x.FieldName == s);
                    replacedValuesCount++;
                    targetTranslationResult.Replace($"[[{s}]]", replacedValue.FieldValue);
                }

                if (replacedValuesCount != fieldsToBeReplaced.Length)
                {
                    throw new Exception("Provided replace values count is different than expected values to be replaced in database dictionary");
                }

                return targetTranslationResult;
            }
            else
            {
                return result.TanslationValue;
            }
        }
    }
}