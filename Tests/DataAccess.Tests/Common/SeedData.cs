using DataAccess.Base;
using DomainModules.BlobStorage.Entities;
using DomainModules.Emails.Entities;

namespace DataAccess.Tests.Common;

public static class SeedData
{
    public static int SeedEmailCount => 2;

    public static Email Email1 { get; set; } = null!;
    public static List<BlobStorageFile> Email1Files { get; set; } = null!;
    public static List<Recipient> Email1Recipients { get; set; } = null!;
    
    public static Email Email2 { get; set; } = null!;
    public static List<BlobStorageFile> Email2Files { get; set; } = null!;
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
        
        Email1Files =
        [
            new BlobStorageFile
            {
                Id = 1,
                FileBaseName = "File1",
                FileExtension = "pdf",
                ContentType = "application/pdf",
                FileContent = new MemoryStream([0x00, 0x01])
            },
            new BlobStorageFile
            {
                Id = 2,
                FileBaseName = "File2",
                FileExtension = "png",
                ContentType = "image/png",
                FileContent = new MemoryStream([0x02, 0x03])
            }
        ];
        await dbContext.Set<BlobStorageFile>().AddRangeAsync(Email1Files);
        
        Email2Files =
        [
            new BlobStorageFile
            {
                Id = 3,
                FileBaseName = "File3",
                FileExtension = "pdf",
                ContentType = "application/pdf",
                FileContent = new MemoryStream([0x04, 0x05])
            },
            new BlobStorageFile
            {
                Id = 4,
                FileBaseName = "File4",
                FileExtension = "png",
                ContentType = "image/png",
                FileContent = new MemoryStream([0x06, 0x07])
            }
        ];
        await dbContext.Set<BlobStorageFile>().AddRangeAsync(Email2Files);
        
        Email1 = new Email
        {
            Id = 1,
            SenderEmailAddress = "sender1@example.com",
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            Recipients = Email1Recipients,
            Attachments = Email1Files
        };
        await dbContext.Set<Email>().AddAsync(Email1);
        
        Email2 = new Email
        {
            Id = 2,
            SenderEmailAddress = "sender2@example.com",
            Subject = "Second Test Email",
            HtmlBody = "This is also a test email.",
            PlainTextBody = "This is also a test email.",
            Recipients = Email2Recipients,
            Attachments = Email2Files
        };
        await dbContext.Set<Email>().AddAsync(Email2);
    }
}