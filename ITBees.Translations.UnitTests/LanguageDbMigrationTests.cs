using System;
using ITBees.Translations.SqlMigration;
using NUnit.Framework;

namespace ITBees.Translations.UnitTests
{
    public class LanguageDbMigrationTests
    {
        [Test]
        public void GetInsertSqlQuerForAllLanaguages_shouldReturnCorrectSqlQueryAtLeastForEnLang_testCouldBeEaslyBroken()
        {
            //Purpose of this test to generate sql and test it directly on database, so nuget deployment could be faster.
            var sql = new LanguageDbMigration().GetInsertSqlQuerForAllLanaguages();
            Console.WriteLine(sql);
            Assert.True(sql.Contains(@"INSERT INTO Language (Id, Code, Name, LanguageType, IsSupported) VALUES ('38', 'en','English', 'EnType', 0) ON DUPLICATE KEY UPDATE Name=Name;"));
        }
    }
}