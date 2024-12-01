using FluentResults;
using OmmelSamvirke.DataAccess.Errors;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.DataAccess.Tests.Common;

[TestFixture, Category("IntegrationTests")]
public class QueryTests : GenericRepositoryTestsBase
{
    // Testing GetById Async
    [Test]
    public async Task GetByIdAsync_EntityExists_ReturnsEntity()
    {
        Result<Email> result = await EmailRepository.GetByIdAsync(SeedData.Email1.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Id, Is.EqualTo(SeedData.Email1.Id));
            Assert.That(result.Value.SenderEmailAddress, Is.EqualTo(SeedData.Email1.SenderEmailAddress));
            Assert.That(result.Value.Recipients, Has.Count.EqualTo(SeedData.Email1.Recipients.Count));
            Assert.That(result.Value.Attachments, Has.Count.EqualTo(SeedData.Email1.Attachments.Count));
        });
    }

    [Test]
    public async Task GetByIdAsync_EntityDoesNotExist_ReturnsNotFoundError()
    {
        Result<Email> result = await EmailRepository.GetByIdAsync(99);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<NotFoundError>());
        });
    }

    [Test]
    public async Task GetByIdAsync_DatabaseException_ReturnsDatabaseError()
    {
        await Context.DisposeAsync(); // Simulate database exception

        Result<Email> result = await EmailRepository.GetByIdAsync(SeedData.Email1.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
    
    // Testing GetByIdsAsync
    [Test]
    public async Task GetByIdsAsync_EntitiesExist_ReturnsEntities()
    {
        var ids = new List<int> { SeedData.Email1.Id, SeedData.Email2.Id };

        Result<List<Email>> result = await EmailRepository.GetByIdsAsync(ids);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(2));
            Assert.That(result.Value.First().Id, Is.EqualTo(SeedData.Email1.Id));
        });
    }
    
    [Test]
    public async Task GetByIdsAsync_SomeEntitiesDoNotExist_ReturnsExistingEntities()
    {
        var ids = new List<int> { SeedData.Email1.Id, 99 };

        Result<List<Email>> result = await EmailRepository.GetByIdsAsync(ids);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task GetByIdsAsync_NoEntitiesExist_ReturnsNotFoundError()
    {
        var ids = new List<int> { 99, 100 };

        Result<List<Email>> result = await EmailRepository.GetByIdsAsync(ids);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<NotFoundError>());
        });
    }
    
    [Test]
    public async Task GetByIdsAsync_DatabaseException_ReturnsDatabaseError()
    {
        var ids = new List<int> { SeedData.Email1.Id };
        await Context.DisposeAsync(); // Simulate database exception

        Result<List<Email>> result = await EmailRepository.GetByIdsAsync(ids);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
    
    // Testing GetAllAsync
    [Test]
    public async Task GetAllAsync_EntitiesExist_ReturnsAllEntities()
    {
        Result<List<Email>> result = await EmailRepository.GetAllAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(Context.Set<Email>().Count()));
        });
    }

    [Test]
    public async Task GetAllAsync_NoEntitiesExist_ReturnsEmptyList()
    {
        Context.Set<Email>().RemoveRange(Context.Set<Email>());
        await Context.SaveChangesAsync();

        Result<List<Email>> result = await EmailRepository.GetAllAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }
    
    [Test]
    public async Task GetAllAsync_DatabaseException_ReturnsDatabaseError()
    {
        await Context.DisposeAsync(); // Simulate database exception

        Result<List<Email>> result = await EmailRepository.GetAllAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
    
    // Testing FindAsync
    [Test]
    public async Task FindAsync_EntitiesMatchPredicate_ReturnsMatchingEntities()
    {
        Result<List<Email>> result = await EmailRepository.FindAsync(e => e.Subject.Contains(SeedData.Email2.Subject));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(1));
            Assert.That(result.Value.First().Id, Is.EqualTo(SeedData.Email2.Id));
        });
    }

    [Test]
    public async Task FindAsync_NoEntitiesMatchPredicate_ReturnsEmptyList()
    {
        Result<List<Email>> result = await EmailRepository.FindAsync(e => e.Subject.Contains("Nonexistent"));
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }
    
    [Test]
    public async Task FindAsync_DatabaseException_ReturnsDatabaseError()
    {
        await Context.DisposeAsync(); // Simulate database exception

        Result<List<Email>> result = await EmailRepository.FindAsync(e => e.Subject.Contains(SeedData.Email1.Subject));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
}
