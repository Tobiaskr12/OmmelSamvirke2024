namespace Contracts.Emails.EmailTemplateEngine;

public  static class Templates
{
    public static class General
    {
        public const string CriticalError = "CriticalError.html";
        public const string EmailServiceLimitAlert = "EmailServiceLimitAlert.html";
    }
    
    public static class ContactLists
    {
        public const string RequestUnsubscribe = "ContactLists/RequestUnsubscribe.html";
        public const string UserRemovedFromContactList = "ContactLists/UserRemovedFromContactList.html";
        
    }
}
