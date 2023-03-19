using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ITBees.Models.Languages;
using ITBees.Translations.Interfaces;
using Newtonsoft.Json.Linq;

namespace ITBees.Translations
{
    public class LanguageJsonGenerator
    {
        private readonly string _languageFilesPath;
        private readonly List<Language> _supportedLanguages;
        
        public LanguageJsonGenerator(string languageFilesPath, List<Language> supportedLanguages)
        {
            _languageFilesPath = languageFilesPath;
            _supportedLanguages = supportedLanguages;
        }

        /// <summary>
        /// For each language defined during class creation, it generates a file (e.g. en.json) containing all keys defined in the classes passed to the method.
        /// </summary>
        /// <param name="translationClasses"></param>
        public void CreateFiles(List<ITranslate> translationClasses)
        {
            foreach (var language in _supportedLanguages)
            {
                var rootObject = new JObject();
                foreach (var translationClass in translationClasses)
                {
                    var classObject = new JObject();
                    PrintAllConstants(translationClass, classObject);
                    rootObject.Add(translationClass.GetType().Name, classObject);
                }

                var path = Path.Combine(_languageFilesPath, $"{language.Code}.json");
                if (new FileInfo(path).Exists)
                {
                    File.Delete(path);
                }

                File.WriteAllText(path, rootObject.ToString());
            }
        }

        private void PrintAllConstants(object classType, JObject jObject)
        {
            Type type = classType.GetType();
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field.IsLiteral || (field.IsInitOnly))
                {
                    var value = field.GetValue(null);
                    string fullClassName = field.DeclaringType.FullName.Replace("+", ".");
                    string fieldName = field.Name;
                    jObject.Add(fullClassName + "." + fieldName, JToken.FromObject(value));
                }
            }

            var internallClasses = type.GetNestedTypes(BindingFlags.Public);
            foreach (var internallClass in internallClasses)
            {
                var nestedObject = new JObject();
                PrintAllConstants(Activator.CreateInstance(internallClass), nestedObject);
                jObject.Add(internallClass.Name, nestedObject);
            }
        }

    }
}