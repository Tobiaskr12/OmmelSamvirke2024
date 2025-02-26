using Contracts.DataAccess;
using FluentResults;
using DataAccess.Errors;
using DomainModules.Emails.Entities;

namespace DataAccess.Tests.Common;

[TestFixture, Category("IntegrationTests")]
public class QueryTests : GenericRepositoryTestsBase
{
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
    
    [Test]
    public async Task GetPaginatedAsync_WithValidPage_ReturnsPaginatedResult()
    {
        const int page = 1;
        const int pageSize = 2;
        int expectedTotalCount = Context.Set<Email>().Count();
        
        Result<PaginatedResult<Email>> result = await EmailRepository.GetPaginatedAsync(page, pageSize);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Items, Has.Count.LessThanOrEqualTo(pageSize));
            Assert.That(result.Value.TotalCount, Is.EqualTo(expectedTotalCount));
        });
    }

    [Test]
    public async Task GetPaginatedAsync_PageOutOfRange_ReturnsEmptyListWithCorrectTotalCount()
    {
        int totalCount = Context.Set<Email>().Count();
        const int pageSize = 5;
        int page = totalCount / pageSize + 2;
        
        Result<PaginatedResult<Email>> result = await EmailRepository.GetPaginatedAsync(page, pageSize);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Items, Is.Empty);
            Assert.That(result.Value.TotalCount, Is.EqualTo(totalCount));
        });
    }

    [Test]
    public async Task GetPaginatedAsync_OrderByDescending_DateCreated()
    {
        int totalCount = Context.Set<Email>().Count();
        int pageSize = totalCount; // Retrieve all items in one page for full order check
        
        Result<PaginatedResult<Email>> result = await EmailRepository.GetPaginatedAsync(1, pageSize);
        
        if (result.Value.Items.Count > 1)
        {
            for (int i = 1; i < result.Value.Items.Count; i++)
            {
                DateTime? dateCreated = result.Value.Items[i].DateCreated;
                
                if (dateCreated != null)
                    Assert.That(result.Value.Items[i - 1].DateCreated, Is.GreaterThanOrEqualTo(dateCreated));
            }
        }
    }

    [Test]
    public async Task GetPaginatedAsync_DatabaseException_ReturnsDatabaseError()
    {
        // Simulate a database exception by disposing the context
        await Context.DisposeAsync();
        
        Result<PaginatedResult<Email>> result = await EmailRepository.GetPaginatedAsync();
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }

    [Test]
    public async Task GetPaginatedAsync_CancellationRequested_ReturnsDatabaseError()
    {
        // Arrange: create a cancellation token that is already cancelled
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        
        Result<PaginatedResult<Email>> result = await EmailRepository.GetPaginatedAsync(1, 1, cancellationToken: cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
}
