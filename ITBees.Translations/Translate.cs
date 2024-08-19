using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using InheritedMapper;
using ITBees.Models.Languages;
using ITBees.Translations.Interfaces;

namespace ITBees.Translations
{
    public static class Translate
    {
        private const int NumberOfSignsInTranslateFileName = 7;
        private static readonly Dictionary<Language, Dictionary<string, string>> AllTranslations = new();

        public static void LoadFiles(string path)
        {
            var files = new DirectoryInfo(path).GetFiles("*.json");
            foreach (var file in files)
                if (file.Name.Length == NumberOfSignsInTranslateFileName)
                {
                    var langPrefix = file.Name.Substring(0, 2);
                    var lang = new DerivedAsTFromStringClassResolver<Language>().GetInstance(langPrefix);
                    var jsonContent = File.ReadAllText(file.FullName);
                    var languageTranslations = new JsonKeyValueService().GenerateKeyValueDictionary(jsonContent);
                    AllTranslations.Add(lang, languageTranslations);
                }
        }

        public static string Get(Type classType, string fieldName, Language language)
        {
            if (!AllTranslations.Any())
                throw new Exception(ITBees.Translations.Translations.TranslateMessages.YouMustLoadTranslationFilesFirst);

            var translateKey = $"{classType.FullName}.{fieldName}".Replace("+", ".");
            var dictionary = AllTranslations
                .FirstOrDefault(x => x.Key.GetType() == language.GetType()).Value;

            if (dictionary != null && dictionary.ContainsKey(translateKey))
            {
                return dictionary[translateKey];
            }

            var fieldValue = GetStaticFieldValue(classType, fieldName);
            if (fieldValue != null)
            {
                return fieldValue;
            }

            throw new Exception(ITBees.Translations.Translations.TranslateMessages.MissingTranslationForSpecifiedKey + $" key : {translateKey}, language :{language.Code}");
        }

        public static string Get(Type classType, string fieldName, string language)
        {
            var lang = new InheritedMapper.DerivedAsTFromStringClassResolver<Language>().GetInstance(language);
            return Get(classType, fieldName, lang);
        }

        public static string Get<T>(Expression<Func<T>> expression, Language language)
        {
            if (AllTranslations.Any() == false)
                throw new Exception(ITBees.Translations.Translations.TranslateMessages.YouMustLoadTranslationFilesFirst);

            if (expression.Body is MemberExpression memberExpression)
            {
                var translateKey = $"{memberExpression.Member.DeclaringType.FullName}.{memberExpression.Member.Name}".Replace("+",".");
                var dictionary = AllTranslations
                    .FirstOrDefault(x => x.Key.GetType() == language.GetType()).Value;
                var translation = dictionary
                    .Where(val => val.Key == translateKey);
                if (translation.Count() == 0)
                    throw new Exception(ITBees.Translations.Translations.TranslateMessages.MissingTranslationForSpecifiedKey + $" key : {translateKey}, language :{language.Code}");
                return translation.First().Value;
            }

            throw new ArgumentException(ITBees.Translations.Translations.TranslateMessages.InvalidExpression, nameof(expression));
        }

        public static string Get<T>(Expression<Func<T>> expression, string language)
        {
            var lang = new InheritedMapper.DerivedAsTFromStringClassResolver<Language>().GetInstance(language);
            return Get<T>(expression, lang);
        }

        public static void ClearTranslations()
        {
            AllTranslations.Clear();
        }

        public static string Get<T>(Expression<Func<T>> expression, string fieldName, Language language)
        {
            if (!AllTranslations.Any())
                throw new Exception(ITBees.Translations.Translations.TranslateMessages.YouMustLoadTranslationFilesFirst);

            if (expression.Body is MemberExpression memberExpression)
            {
                var classType = memberExpression.Member.DeclaringType; 
                var translateKey = $"{classType.FullName}.{fieldName}".Replace("+", ".");

                var dictionary = AllTranslations
                    .FirstOrDefault(x => x.Key.GetType() == language.GetType()).Value;

                if (dictionary != null && dictionary.ContainsKey(translateKey))
                {
                    return dictionary[translateKey];
                }

                var fieldValue = GetStaticFieldValue(classType, fieldName);
                if (fieldValue != null)
                    return fieldValue;

                throw new Exception(ITBees.Translations.Translations.TranslateMessages.MissingTranslationForSpecifiedKey + $" key : {translateKey}, language :{language.Code}");
            }

            throw new ArgumentException(ITBees.Translations.Translations.TranslateMessages.InvalidExpression, nameof(expression));
        }

        public static string Get<T>(Expression<Func<T>> expression, string fieldName, string language)
        {
            var lang = new InheritedMapper.DerivedAsTFromStringClassResolver<Language>().GetInstance(language);
            return Get(expression, fieldName, lang);
        }

        public static string GetStaticFieldValue(Type classType, string fieldName)
        {
            var field = classType.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
            if (field != null)
            {
                return field.GetValue(null)?.ToString();
            }

            foreach (var nestedType in classType.GetNestedTypes())
            {
                field = nestedType.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
                if (field != null)
                {
                    return field.GetValue(null)?.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Allows to create translation files in form of json (ie. en.json, de.json etc...)
        /// </summary>
        /// <param name="path">Enter path to folder where generated translations will be stored</param>
        /// <param name="tranlateClasses">Give all translation classes to be generated, remember that all this classes have to implement marker interface ITranslate and has specific structure described in documentation</param>
        /// <param name="supportedLanguages">Enter list of languages supported, so there will be corresponding json files generated ie en.json, de.json etc.</param>
        /// <param name="overrideTranslationFileIfExists">If target file ie. en.json already exists on disk it will be overwritten with default values, so be aware of possible data loss</param>
        /// <exception cref="NotImplementedException"></exception>
        public static void Configure(string path, 
            List<ITranslate> tranlateClasses, 
            List<Language> supportedLanguages,
            bool overrideTranslationFileIfExists)
        {
            var generator = new LanguageJsonGenerator(path, supportedLanguages);
            generator.CreateFiles(tranlateClasses, overrideTranslationFileIfExists);
        }
    }
}