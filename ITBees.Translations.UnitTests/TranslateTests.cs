using System;
using System.Collections.Generic;
using System.IO;
using ITBees.Models.Languages;
using ITBees.Translations.Interfaces;
using ITBees.Translations.Translations;
using NUnit.Framework;

namespace ITBees.Translations.UnitTests
{
    public class TranslateTests
    {
        [TearDown]
        public void TearDown()
        {
            Translate.ClearTranslations();
        }

        [NonParallelizable]
        //[Test]
        public void Get_shouldReturnCorrectTranslatedValueForSpecifiedLanguage()
        {
            Translate.ClearTranslations();
            var languageFilesPath = Path.Combine(Environment.CurrentDirectory, "i18n");
            var generator = new LanguageJsonGenerator(languageFilesPath,
                new List<Language>() { new En(), new Pl() });

            generator.CreateFiles(new List<ITranslate>() { new TranslateSampleTestClass() }, true);

            var polishTranslationValue = ReplaceTestValuesInTranslatedFileForSimulatingPolishTranslation(languageFilesPath, out var polishTranslationKeyAndValue);

            Translate.LoadFiles(languageFilesPath);

            var translatedTextInPolish = Translate.Get(() => ITBees.Translations.Translations.TranslateMessages.InvalidExpression, new Pl());

            Assert.True(translatedTextInPolish == polishTranslationValue, $"Expected translation was : {polishTranslationKeyAndValue}, but received {translatedTextInPolish}");
        }

        [NonParallelizable]
        //[Test]
        public void Get_shouldReturnCorrectTranslatedValueForSpecifiedLanguageInString()
        {
            Translate.ClearTranslations();
            var languageFilesPath = Path.Combine(Environment.CurrentDirectory, "i18n");
            var generator = new LanguageJsonGenerator(languageFilesPath,
                new List<Language>() { new En(), new Pl() });

            generator.CreateFiles(new List<ITranslate>() { new Translations.TranslateMessages(), new TranslateSampleTestClass() }, true);

            var polishTranslationValue = ReplaceTestValuesInTranslatedFileForSimulatingPolishTranslation(languageFilesPath, out var polishTranslationKeyAndValue);

            Translate.LoadFiles(languageFilesPath);

            var translatedTextInPolish = Translate.Get(() => ITBees.Translations.Translations.TranslateMessages.InvalidExpression, "pl");

            Assert.True(translatedTextInPolish == polishTranslationValue, $"Expected translation was : {polishTranslationKeyAndValue}, but received {translatedTextInPolish}");
        }

        private string ReplaceTestValuesInTranslatedFileForSimulatingPolishTranslation(string languageFilesPath,
            out string polishTranslationKeyAndValue)
        {
            var polishFilePath = Path.Combine(languageFilesPath, "pl.json");
            var polishFileContent = File.ReadAllText(polishFilePath);
            Console.WriteLine(polishFileContent);
            var polishTranslationValue = @"Nie prawidlowa wartosc!";
            polishTranslationKeyAndValue =
                @"ITBees.Translations.FAS.UserManager.NewUserRegistration.ToAddNewUserYouMustBeCompanyOwner"": """ +
                polishTranslationValue + @"""";
            polishFileContent = polishFileContent.Replace(
                @"ITBees.Translations.FAS.UserManager.NewUserRegistration.ToAddNewUserYouMustBeCompanyOwner"": ""To add new user You must be company owner!""",
                polishTranslationKeyAndValue);
            File.WriteAllText(polishFilePath, polishFileContent);
            return polishTranslationValue;
        }

        [NonParallelizable]
        //[Test]
        public void TranslateGet_shouldThrowErrorTranslationsWasNotLoadedFirs()
        {
            Assert.Throws<Exception>(() => Translate.Get(() => "Test", new En()));
        }

        [NonParallelizable]
        //[Test]
        public void TranslateConfigure_shouldCreateFolderWithFiles()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "i18n");
            List<ITranslate> tranlateClasses = new List<ITranslate>()
                {new TranslateMessages()};
            var supportedLanguages = new List<Language>() { new Pl(), new En(), new De() };
            var ovverideTranslationFileIfExists = true;

            Translate.Configure(path, tranlateClasses, supportedLanguages, ovverideTranslationFileIfExists);

            var files = new DirectoryInfo(path).GetFiles();
            Assert.True(files.Length == supportedLanguages.Count);
        }
    }
}