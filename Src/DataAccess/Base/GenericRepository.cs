using System.Linq.Expressions;
using Contracts.DataAccess;
using Contracts.DataAccess.Base;
using Contracts.SupportModules.Logging;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DataAccess.Errors;
using DomainModules.Common;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataAccess.Base;

public sealed class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly OmmelSamvirkeDbContext _context;
    private readonly ILoggingHandler _logger;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(OmmelSamvirkeDbContext context, ILoggingHandler logger)
    {
        _context = context;
        _logger = logger;
        _dbSet = context.Set<T>();
    }
    
    public async Task<Result<T>> GetByIdAsync(int id, bool readOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"GetByIdAsync called with id: {id}");
        try
        {
            IQueryable<T> query = BuildQuery(predicate: e => e.Id == id, readOnly);
            T? entity = await query.SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (entity != null) return Result.Ok(entity);
            
            _logger.LogInformation($"Entity with ID {id} not found.");
            return Result.Fail<T>(new NotFoundError($"Entity with ID {id} not found."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while getting entity by id: {id}");
            return Result.Fail<T>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }
    
    public async Task<Result<List<T>>> GetByIdsAsync(List<int> ids, bool readOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"GetByIdsAsync called with ids: {ids}");
        try
        {
            IQueryable<T> query = BuildQuery(predicate: e => ids.Contains(e.Id), readOnly);

            List<T> entities = await query.ToListAsync(cancellationToken: cancellationToken);

            if (entities.Count != 0) return Result.Ok(entities);
            
            _logger.LogInformation($"No entities found with the provided IDs: {ids}");
            return Result.Fail<List<T>>(new NotFoundError("No entities found with the provided IDs."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while getting entities by ids: {ids}");
            return Result.Fail<List<T>>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }
    
    public async Task<Result<List<T>>> GetAllAsync(bool readOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllAsync called.");
        try
        {
            IQueryable<T> query = BuildQuery(readOnly: readOnly);
            List<T> entities = await query.ToListAsync(cancellationToken: cancellationToken);
            
            return Result.Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting all entities.");
            return Result.Fail<List<T>>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }

    public async Task<Result<PaginatedResult<T>>> GetPaginatedAsync(int page = 1, int pageSize = 20, bool readOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetPaginatedAsync called.");
        try
        {
            IQueryable<T> query = BuildQuery(readOnly: readOnly);

            Task<int> countQuery = query.CountAsync(cancellationToken); 
            Task<List<T>> entitiesQuery = query
                .OrderByDescending(x => x.DateCreated)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken: cancellationToken);

            await Task.WhenAll(countQuery, entitiesQuery);

            return Result.Ok(new PaginatedResult<T>
            {
                Items = entitiesQuery.Result,
                TotalCount = countQuery.Result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting paginated entities.");
            return Result.Fail<PaginatedResult<T>>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }

    public async Task<Result<List<T>>> FindAsync(
        Expression<Func<T, bool>>? predicate,
        bool readOnly = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FindAsync called.");
        try
        {
            IQueryable<T> query = BuildQuery(predicate: predicate, readOnly: readOnly);
            List<T> entities = await query.ToListAsync(cancellationToken: cancellationToken);
            
            return Result.Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while finding entities.");
            return Result.Fail<List<T>>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }

    public async Task<Result<T>> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AddAsync called for entity.");
        try
        {
            SetNavigationPropertiesAsUnchanged(entity);

            EntityEntry<T> entry = await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok(entry.Entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding entity.");
            return Result.Fail<T>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }
    
    public async Task<Result<List<T>>> AddAsync(List<T> entities, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AddAsync called for entities.");
        try
        {
            foreach (T entity in entities)
            {
                SetNavigationPropertiesAsUnchanged(entity);
            }
        
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding entities.");
            return Result.Fail<List<T>>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }

    public async Task<Result<T>> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"UpdateAsync called for entity with ID {entity.Id}.");
        try
        {
            EntityEntry<T> entry = _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok(entry.Entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating entity with ID {entity.Id}.");
            return Result.Fail<T>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }
    
    public async Task<Result<List<T>>> UpdateAsync(List<T> entities, CancellationToken cancellationToken = default)
    {
        List<int> entityIds = entities.Select(e => e.Id).ToList();
        _logger.LogInformation($"UpdateAsync called for entities with IDs {entityIds}.");
        try
        {
            List<int> existingIds = await _dbSet
                .Where(e => entityIds.Contains(e.Id))
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);
            
            List<T> validEntities = entities.Where(e => existingIds.Contains(e.Id)).ToList();
            List<T> invalidEntities = entities.Where(e => !existingIds.Contains(e.Id)).ToList();

            var errors = new List<Error>();

            if (invalidEntities.Count != 0)
            {
                errors.Add(new NotFoundError($"Entities with IDs {string.Join(", ", invalidEntities.Select(e => e.Id))} not found."));
            }

            if (validEntities.Count != 0)
            {
                _dbSet.UpdateRange(validEntities);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return errors.Count != 0 
                ? Result.Fail<List<T>>(errors) 
                : Result.Ok(validEntities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating entities with IDs {entityIds}.");
            return Result.Fail<List<T>>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }

    public async Task<Result> DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"DeleteAsync called for entity with ID {entity.Id}.");
        try
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting entity with ID {entity.Id}.");
            return Result.Fail(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }
    
    public async Task<Result> DeleteAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        List<T> entityList = entities.ToList();
        List<int> entityIds = entityList.Select(e => e.Id).ToList();
        _logger.LogInformation($"DeleteAsync called for entities with IDs {entityIds}.");

        try
        {
            List<T> existingEntities = 
                await _dbSet
                    .Where(e => entityIds.Contains(e.Id))
                    .ToListAsync(cancellationToken);
            
            List<T> nonexistentEntities = 
                entityList
                    .Where(e => existingEntities.All(ee => ee.Id != e.Id))
                    .ToList();
            
            if (existingEntities.Count != 0)
            {
                _dbSet.RemoveRange(existingEntities);
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting existing entities with IDs {existingEntities.Select(e => e.Id)}.");
                return Result.Fail(new DatabaseError($"An error occurred: {ex.Message}"));
            }

            if (nonexistentEntities.Count == 0)
            {
                return Result.Ok();
            }

            List<int> nonexistentIds = nonexistentEntities.Select(e => e.Id).ToList();
            _logger.LogInformation($"Entities with IDs {nonexistentIds} do not exist and could not be deleted.");
            List<string> errorMessages = 
                nonexistentEntities
                    .Select(e => $"Entity with ID {e.Id} does not exist.")
                    .ToList();
            return Result.Fail(new DatabaseError(string.Join("; ", errorMessages)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting entities with IDs {entityIds}.");
            return Result.Fail(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }

    /// <summary>
    /// Builds a query from the specified parameters.
    /// </summary>
    /// <param name="predicate">An optional filter expression.</param>
    /// <param name="readOnly">Determines if the returned entities should be tracked or not.
    /// The value true disables tracking</param>
    /// <returns>A <see cref="IQueryable{T}"/> representing the query.</returns>
    private IQueryable<T> BuildQuery(Expression<Func<T, bool>>? predicate = null, bool readOnly = true)
    {
        IQueryable<T> query = _dbSet;

        if (readOnly)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        // Dynamically include all navigation properties.
        IEntityType? entityType = _context.Model.FindEntityType(typeof(T));
        IEnumerable<INavigation> navigationProperties = entityType?.GetNavigations() ?? [];
        IEnumerable<ISkipNavigation> skipNavigations = entityType?.GetSkipNavigations() ?? []; // Many-to-many relationships
        
        foreach (INavigation navigation in navigationProperties)
        {
            query = query.Include(navigation.Name);
        }
        
        foreach (ISkipNavigation skipNavigation in skipNavigations)
        {
            query = query.Include(skipNavigation.Name);
        }

        return query;
    }
    
    private void SetNavigationPropertiesAsUnchanged(T entity)
    {
        EntityEntry<T> entry = _context.Entry(entity);
        
        foreach (NavigationEntry navigation in entry.Navigations)
        {
            if (navigation.CurrentValue is IEnumerable<object> collection)
            {
                foreach (object child in collection)
                {
                    EntityEntry childEntry = _context.Entry(child);
                    if (childEntry.State == EntityState.Added)
                    {
                        childEntry.State = EntityState.Unchanged;
                    }
                }
            }
            else
            {
                object? navEntity = navigation.CurrentValue;
                if (navEntity != null)
                {
                    EntityEntry navEntry = _context.Entry(navEntity);
                    if (navEntry.State == EntityState.Added)
                    {
                        navEntry.State = EntityState.Unchanged;
                    }
                }
            }
        }
    }
}
