using FluentResults;
using OmmelSamvirke.DataAccess.Errors;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.DataAccess.Tests.Common;

[TestFixture, Category("IntegrationTests")]
public class DeleteTests : GenericRepositoryTestsBase
{
    [Test]
    public async Task Delete_ExistingEntity_DeletesEntity()
    {
        Result<Email> emailResult = await EmailRepository.GetByIdAsync(SeedData.Email1.Id, readOnly: false);
        Email? email = emailResult.Value;

        Result deleteResult = await EmailRepository.DeleteAsync(email);
        Result<Email> getResult = await EmailRepository.GetByIdAsync(SeedData.Email1.Id);

        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.IsSuccess, Is.True);
            Assert.That(getResult.IsSuccess, Is.False);
            Assert.That(getResult.Errors.First(), Is.InstanceOf<NotFoundError>());
        });
    }

    [Test]
    public async Task Delete_NonexistentEntity_ReturnsDatabaseError()
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

        Result deleteResult = await EmailRepository.DeleteAsync(email);

        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.IsSuccess, Is.False);
            Assert.That(deleteResult.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
    
    [Test]
    public async Task Delete_DatabaseException_ReturnsDatabaseError()
    {
        await Context.DisposeAsync(); // Simulate database exception
        Result<Email> result = await EmailRepository.DeleteAsync(new Email
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
    public async Task DeleteAsync_ExistingEntities_DeletesEntities()
    {
        var emailsToDelete = new List<Email>
        {
            SeedData.Email1,
            SeedData.Email2
        };

        Result deleteResult = await EmailRepository.DeleteAsync(emailsToDelete);
        Result<Email> getResult1 = await EmailRepository.GetByIdAsync(SeedData.Email1.Id);
        Result<Email> getResult2 = await EmailRepository.GetByIdAsync(SeedData.Email2.Id);

        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.IsSuccess, Is.True);
            Assert.That(getResult1.IsSuccess, Is.False);
            Assert.That(getResult1.Errors.First(), Is.InstanceOf<NotFoundError>());
            Assert.That(getResult2.IsSuccess, Is.False);
            Assert.That(getResult2.Errors.First(), Is.InstanceOf<NotFoundError>());
        });
    }

    [Test]
    public async Task DeleteAsync_SomeNonexistentEntities_ReturnsPartialSuccess()
    {
        Result<Email> email1Result = await EmailRepository.GetByIdAsync(SeedData.Email1.Id, readOnly: false);
        Email email1 = email1Result.Value;
        
        List<Email> emailsToDelete =
        [
            email1,
            new Email
            {
                Id = 99, // Nonexistent ID
                SenderEmailAddress = "nonexistent@example.com",
                Subject = "Nonexistent Email",
                Body = "This email does not exist.",
                Recipients = [],
                Attachments = []
            }
        ];

        Result deleteResult = await EmailRepository.DeleteAsync(emailsToDelete);
        Result<Email> getResult1 = await EmailRepository.GetByIdAsync(SeedData.Email1.Id);

        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.IsSuccess, Is.False);
            Assert.That(deleteResult.Errors, Is.Not.Empty);
            
            Assert.That(getResult1.IsSuccess, Is.False);
            Assert.That(getResult1.Errors.First(), Is.InstanceOf<NotFoundError>());
        });
    }

    [Test]
    public async Task DeleteAsync_BulkDatabaseException_ReturnsDatabaseError()
    {
        await Context.DisposeAsync(); // Simulate database exception
        var emailsToDelete = new List<Email>
        {
            SeedData.Email1,
            SeedData.Email2
        };

        Result deleteResult = await EmailRepository.DeleteAsync(emailsToDelete);

        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.IsSuccess, Is.False);
            Assert.That(deleteResult.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
}
