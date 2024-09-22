namespace EmailWrapper.Constants;

public static class SenderEmailAddresses
{
    public static string Auto => "auto@ommelsamvirke.com";
    public static string Admins => "admins@ommelsamvirke.com";
    public static string Auth => "auth@ommelsamvirke.com";
    public static string Newsletter => "nyhedsbrev@ommelsamvirke.com";

    public static IReadOnlyList<string> AcceptedEmailAddresses { get; } = [Auto, Admins, Auth, Newsletter];
}
