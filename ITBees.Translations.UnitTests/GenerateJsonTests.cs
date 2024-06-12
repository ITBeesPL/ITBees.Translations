using ITBees.Models.Languages;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using ITBees.Translations.Interfaces;

namespace ITBees.Translations.UnitTests
{
    public class GenerateJsonTests
    {
        [Test]
        public void CreateFiles_ShouldCreateAllJsonLanguageFilesSpecifiedInConstructor()
        {
            var rootFolder = Directory.GetCurrentDirectory();
            var languageFilesPath = Path.Combine(rootFolder, "i18n");

            if (Directory.Exists(languageFilesPath))
            {
                Directory.Delete(languageFilesPath, true);
            }

            Directory.CreateDirectory(languageFilesPath);

            var supportedLanguages = new List<Language>() { new En(), new Pl(), new De() };
            var generator = new LanguageJsonGenerator(languageFilesPath, supportedLanguages);

            generator.CreateFiles(new List<ITranslate>() {new Translations.TranslateMessages()}, true);

            var generatedFilesCount = new DirectoryInfo(languageFilesPath).GetFiles("*.json").Length;
            Assert.That(generatedFilesCount == supportedLanguages.Count, $"Expected files to be generated : {supportedLanguages.Count} but was {generatedFilesCount}, debug folder : {languageFilesPath}");
        }
    }
}