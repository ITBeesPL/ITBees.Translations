using System;
using System.Text;
using ITBees.Models.Languages;

namespace ITBees.Translations.SqlMigration
{
    public class LanguageDbMigration
    {
        public string GetInsertSqlQuerForAllLanaguages()
        {
            var sb = new StringBuilder();
            foreach (var type in InheritedMapper.BaseClassHelper.GetAllDerivedClassesFromBaseClass(typeof(Language)))
            {
                var instance = Activator.CreateInstance(type) as Language;
                sb.AppendLine($"INSERT INTO Language (Id, Code, Name, LanguageType, IsSupported) VALUES ('{instance.Id}', '{instance.Code}','{instance.Name}', '{instance.GetType().Name}Type', 0) ON DUPLICATE KEY UPDATE Name=Name; ");
            }

            return sb.ToString();
        }
    }
}