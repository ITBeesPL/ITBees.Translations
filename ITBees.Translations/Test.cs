using ITBees.Models.Languages;
using ITBees.Translations;
using ITBees.Translations.Translations;

class Test
{
    public Test()
    {
        var t = Translate.Get(typeof(TranslateMessages), "a", new Aa(), true);
    }
}