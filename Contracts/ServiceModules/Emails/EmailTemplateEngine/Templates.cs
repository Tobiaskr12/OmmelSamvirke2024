namespace Contracts.ServiceModules.Emails.EmailTemplateEngine;

public  static class Templates
{
    public static class General
    {
        public const string CriticalError = "CriticalErrorLog.html";
        public const string EmailServiceLimitAlert = "EmailServiceLimitAlert.html";
    }
    
    public static class ContactLists
    {
        public const string RequestUnsubscribe = "ContactLists/RequestUnsubscribe.html";
        public const string UserRemovedFromContactList = "ContactLists/UserRemovedFromContactList.html";
        public const string Message = "ContactLists/Message.html";
    }

    public static class Newsletters
    {
        public const string CleanupCampaign = "Newsletters/CleanupCampaign.html";
        public const string CleanupNotice = "Newsletters/CleanupNotice.html";
        public const string ConfirmNewsletterSubscription = "Newsletters/ConfirmNewsletterSubscription.html";
        public const string ConfirmNewsletterUnsubscription = "Newsletters/ConfirmNewsletterUnsubscription.html";
        public const string Newsletter = "Newsletters/Newsletter.html";
    }
    
    public static class Reservations
    {
        public const string ReservationRequestConfirmation = "Reservations/ReservationRequestConfirmation.html";
        public const string ReservationApprovedNotification = "Reservations/ReservationApprovedNotification.html";
        public const string ReservationDeclinedNotification = "Reservations/ReservationDeclinedNotification.html";
    }
}
