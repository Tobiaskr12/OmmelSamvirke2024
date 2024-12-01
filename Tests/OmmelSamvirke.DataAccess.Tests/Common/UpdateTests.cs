using FluentResults;
using OmmelSamvirke.DataAccess.Errors;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.DataAccess.Tests.Common;

[TestFixture, Category("IntegrationTests")]
public class UpdateTests : GenericRepositoryTestsBase
{
    [Test]
    public async Task Update_ExistingEntity_UpdatesEntity()
    {
        const string updatedSubject = "Updated Subject";
        Result<Email> emailResult = await EmailRepository.GetByIdAsync(SeedData.Email1.Id, readOnly: false);
        Email? email = emailResult.Value;
        email.Subject = updatedSubject;

        Result<Email> updateResult = await EmailRepository.UpdateAsync(email);
        
        Assert.Multiple(() =>
        {
            Assert.That(updateResult.IsSuccess, Is.True);
            Assert.That(updateResult.Value.Subject, Is.EqualTo(updatedSubject));
        });
    }

    [Test]
    public async Task Update_NonexistentEntity_ReturnsDatabaseError()
    {
        var email = new Email
        {
            Id = 99, // Nonexistent ID
            SenderEmailAddress = "nonexistent@example.com",
            Subject = "Nonexistent Email",
            Body = "This email does not exist.",
            Recipients = [],
            Attachments = []
        };

        Result<Email> updateResult = await EmailRepository.UpdateAsync(email);

        Assert.Multiple(() =>
        {
            Assert.That(updateResult.IsSuccess, Is.False);
            Assert.That(updateResult.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
    
    [Test]
    public async Task Update_DatabaseException_ReturnsDatabaseError()
    {
        await Context.DisposeAsync(); // Simulate database exception
        Result<Email> result = await EmailRepository.UpdateAsync(new Email
        {
            SenderEmailAddress = "Test data",
            Subject = "Test data",
            Body = "Test data",
            Recipients = [],
            Attachments = []
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
    
    [Test]
    public async Task UpdateAsync_ValidEntities_UpdatesEntities()
    {
        Result<List<Email>> getResult = await EmailRepository.GetByIdsAsync(
            [SeedData.Email1.Id, SeedData.Email2.Id],
            readOnly: false);
        List<Email>? emails = getResult.Value;
        
        emails[0].Subject = "Bulk Updated Subject 1";
        emails[1].Subject = "Bulk Updated Subject 2";

        Result<List<Email>> updateResult = await EmailRepository.UpdateAsync(emails);
        Result<List<Email>> verifyResult = await EmailRepository.GetByIdsAsync([SeedData.Email1.Id, SeedData.Email2.Id]);

        Assert.Multiple(() =>
        {
            Assert.That(updateResult.IsSuccess, Is.True);
            Assert.That(updateResult.Value, Has.Count.EqualTo(2));
            Assert.That(updateResult.Value[0].Subject, Is.EqualTo("Bulk Updated Subject 1"));
            Assert.That(updateResult.Value[1].Subject, Is.EqualTo("Bulk Updated Subject 2"));
            
            Assert.That(verifyResult.IsSuccess, Is.True);
            Assert.That(verifyResult.Value[0].Subject, Is.EqualTo("Bulk Updated Subject 1"));
            Assert.That(verifyResult.Value[1].Subject, Is.EqualTo("Bulk Updated Subject 2"));
        });
    }

    [Test]
    public async Task UpdateAsync_SomeInvalidEntities_ReturnsPartialSuccess()
    {
        Result<Email> email1Result = await EmailRepository.GetByIdAsync(SeedData.Email1.Id, readOnly: false);
        Email email1 = email1Result.Value;
        email1.SenderEmailAddress = "updated1@example.com";
        email1.Subject = "Valid Update 1";
        email1.Body = "Updated body 1";
        
        List<Email> emailsToUpdate =
        [
            email1,
            new Email
            {
                Id = 99, // Nonexistent ID
                SenderEmailAddress = "invalid@example.com",
                Subject = "Invalid Update",
                Body = "This update should fail.",
                Recipients = null!,
                Attachments = null!
            }
        ];

        Result<List<Email>> updateResult = await EmailRepository.UpdateAsync(emailsToUpdate);
        Result<Email> updatedEmail = await EmailRepository.GetByIdAsync(SeedData.Email1.Id);

        Assert.Multiple(() =>
        {
            Assert.That(updateResult.IsSuccess, Is.False);
            Assert.That(updateResult.Errors, Is.Not.Empty);

            Assert.That(updatedEmail.IsSuccess, Is.True);
            Assert.That(updatedEmail.Value.SenderEmailAddress, Is.EqualTo(email1.SenderEmailAddress));
            Assert.That(updatedEmail.Value.Subject, Is.EqualTo(email1.Subject));
            Assert.That(updatedEmail.Value.Body, Is.EqualTo(email1.Body));
        });
    }

    [Test]
    public async Task UpdateAsync_BulkDatabaseException_ReturnsDatabaseError()
    {
        await Context.DisposeAsync(); // Simulate database exception
        List<Email> emailsToUpdate =
        [
            new Email
            {
                Id = SeedData.Email1.Id,
                SenderEmailAddress = "bulk_error1@example.com",
                Subject = "Bulk Error Update 1",
                Body = "This update will fail.",
                Recipients = SeedData.Email1Recipients,
                Attachments = SeedData.Email1Attachments
            },

            new Email
            {
                Id = SeedData.Email2.Id,
                SenderEmailAddress = "bulk_error2@example.com",
                Subject = "Bulk Error Update 2",
                Body = "This update will also fail.",
                Recipients = SeedData.Email2Recipients,
                Attachments = SeedData.Email2Attachments
            }
        ];

        Result<List<Email>> result = await EmailRepository.UpdateAsync(emailsToUpdate);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
}
