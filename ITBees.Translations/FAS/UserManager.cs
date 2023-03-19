using ITBees.Translations.Interfaces;

namespace ITBees.Translations.FAS
{
    public class UserManager : ITranslate
    {
        public class NewUserRegistration : ITranslate
        {
            /// <summary>
            /// Error message shown when user tries to add different user when he is not owner of this company
            /// </summary>
            public static readonly string ToAddNewUserYouMustBeCompanyOwner = "To add new user You must be company owner!";
            /// <summary>
            /// If user try's to register account for another person, and is not logged in, or his token expired
            /// </summary>
            public static readonly string IfYouWantToAddNewUserToCompany = "If You want to add new user to company, you have to be authenticated!";
            /// <summary>
            /// Default company name set if user has not set own company name (only simple account registration)
            /// </summary>
            public static readonly string DefaultPrivateCompanyName = "Private";
        }
    }
}