using System;
using System.Collections.Generic;
using System.Linq;
using ITBees.Interfaces.Repository;
using ITBees.Models.Languages;
using ITBees.Translations.Interfaces;
using ITBees.Translations.SqlMigration;

namespace ITBees.Translations.Services
{
    public class RuntimeTranslationService : IRuntimeTranslationService
    {
        private readonly IReadOnlyRepository<RuntimeTranslation> _roRepoRuntimeTranslation;

        public RuntimeTranslationService(IReadOnlyRepository<RuntimeTranslation> roRepoRuntimeTranslation)
        {
            _roRepoRuntimeTranslation = roRepoRuntimeTranslation;
        }
        public string GetTranslation(string key, Language lang, List<ReplaceableValue> replaceableValues = null)
        {
            var translation = _roRepoRuntimeTranslation.GetData(x => x.TranslationKey == key && x.LanguageId == lang.Id).ToList();
            if (translation.Count > 1)
            {
                throw new Exception("To many translation for selected key");
            }

            if (translation.Any() == false)
            {
                throw new Exception($"There is no translation for key :{key} and language :{lang.Code}");
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