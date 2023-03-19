using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using InheritedMapper;
using ITBees.Models.Languages;

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

        public static string Get<T>(Expression<Func<T>> expression, Language language)
        {
            if (AllTranslations.Any() == false)
                throw new Exception("You must load translation files firs, use method : Translate.LoadFiles(path);");

            if (expression.Body is MemberExpression memberExpression)
            {
                var translateKey = $"{memberExpression.Member.DeclaringType.FullName}.{memberExpression.Member.Name}".Replace("+",".");
                var dictionary = AllTranslations
                    .FirstOrDefault(x => x.Key.GetType() == language.GetType()).Value;
                var translation = dictionary
                    .Where(val => val.Key == translateKey);
                return translation.First().Value;
            }

            throw new ArgumentException("Invalid expression", nameof(expression));
        }

        public static void ClearTranslations()
        {
            AllTranslations.Clear();
        }
    }
}