namespace Emails.Domain.Constants;

public static class ServiceLimits
{
    public static int EmailsPerMinute => 5_000;
    public static int EmailsPerHour => 20_000;
    public static int RecipientsPerEmail => 50;
    public static int MaxEmailRequestSizeInBytes => 7_864_320; // 7.5MB
}
