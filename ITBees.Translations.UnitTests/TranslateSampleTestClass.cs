using ITBees.Translations.Interfaces;

namespace ITBees.Translations.UnitTests
{
    public class TranslateSampleTestClass : ITranslate
    {
        public static readonly string TestField1 = "TestField 1";
        public static readonly string TestField2 = "TestField 2";

        public class NestedClass : ITranslate
        {
            public static readonly string NestedFiled1 = "Nested Field 1";

            public class NestedInsideNestedClass : ITranslate
            {
                public static readonly string NestedInsideNestedFiled1 = "Nested Inside Nested Filed 1";
            }
        }
    }
}