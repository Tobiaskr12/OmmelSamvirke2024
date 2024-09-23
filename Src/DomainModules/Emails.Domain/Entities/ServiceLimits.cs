namespace Emails.Domain.Entities;

public static class ServiceLimits
{
    public static int EmailsPerMinut => 5_000;
    public static int EmailsPerHour => 20_000;
}
