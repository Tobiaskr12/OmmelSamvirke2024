namespace DomainModules.Emails.Constants;

public static class ServiceLimits
{
    public const int EmailsPerMinute = 5_000;
    public const int EmailsPerHour = 20_000;
    public const int RecipientsPerEmail = 50;
    public const int MaxEmailRequestSizeInBytes = 7_864_320; // 7.5MB
}
