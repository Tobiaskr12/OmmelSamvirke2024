namespace EmailWrapper.Tests.Models;

public class TestEmailClient
{
    public required string EmailAddress { get; set; }
    public required string AccountPassword { get; set; }
    public required string ImapHost { get; set; }
    public required int ImapPort { get; set; }
}
