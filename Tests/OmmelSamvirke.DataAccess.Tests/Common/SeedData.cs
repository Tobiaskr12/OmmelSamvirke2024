using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.DataAccess.Tests.Common;

public static class SeedData
{
    public static int SeedEmailCount { get; set; } = 2;
    
    public static Email Email1 { get; set; } = null!;
    public static List<Attachment> Email1Attachments { get; set; } = null!;
    public static List<Recipient> Email1Recipients { get; set; } = null!;
    
    public static Email Email2 { get; set; } = null!;
    public static List<Attachment> Email2Attachments { get; set; } = null!;
    public static List<Recipient> Email2Recipients { get; set; } = null!;
    
    public static async Task AddSeed(OmmelSamvirkeDbContext dbContext)
    {
        Email1Recipients =
        [
            new Recipient { Id = 1, EmailAddress = "recipient1@example.com" },
            new Recipient { Id = 2, EmailAddress = "recipient2@example.com" }
        ];
        await dbContext.Set<Recipient>().AddRangeAsync(Email1Recipients);
        
        Email2Recipients =
        [
            new Recipient { Id = 3, EmailAddress = "recipient3@example.com" },
            new Recipient { Id = 4, EmailAddress = "recipient4@example.com" }
        ];
        await dbContext.Set<Recipient>().AddRangeAsync(Email2Recipients);
        
        Email1Attachments =
        [
            new Attachment
            {
                Id = 1,
                Name = "Attachment1",
                ContentPath = new Uri("https://example.com/attachment1"),
                ContentType = new ContentType("application/pdf"),
                BinaryContent = [0x00, 0x01],
                EmailId = 1
            },

            new Attachment
            {
                Id = 2,
                Name = "Attachment2",
                ContentPath = new Uri("https://example.com/attachment2"),
                ContentType = new ContentType("image/png"),
                BinaryContent = [0x02, 0x03],
                EmailId = 1
            }
        ];
        await dbContext.Set<Attachment>().AddRangeAsync(Email1Attachments);
        
        Email2Attachments =
        [
            new Attachment
            {
                Id = 3,
                Name = "Attachment3",
                ContentPath = new Uri("https://example.com/attachment3"),
                ContentType = new ContentType("application/pdf"),
                BinaryContent = [0x04, 0x05],
                EmailId = 2
            },

            new Attachment
            {
                Id = 4,
                Name = "Attachment4",
                ContentPath = new Uri("https://example.com/attachment4"),
                ContentType = new ContentType("image/png"),
                BinaryContent = [0x06, 0x07],
                EmailId = 2
            }
        ];
        await dbContext.Set<Attachment>().AddRangeAsync(Email2Attachments);
        
        Email1 = new Email
        {
            Id = 1,
            SenderEmailAddress = "sender1@example.com",
            Subject = "Test Email",
            Body = "This is a test email.",
            Recipients = Email1Recipients,
            Attachments = Email1Attachments
        };
        await dbContext.Set<Email>().AddAsync(Email1);
        
        Email2 = new Email
        {
            Id = 2,
            SenderEmailAddress = "sender2@example.com",
            Subject = "Second Test Email",
            Body = "This is also a test email.",
            Recipients = Email2Recipients,
            Attachments = Email2Attachments
        };
        await dbContext.Set<Email>().AddAsync(Email2);
    }
}
