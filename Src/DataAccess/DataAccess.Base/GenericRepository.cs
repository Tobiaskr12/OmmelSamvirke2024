using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using FluentResults;
using System.Linq.Expressions;
using DataAccess.Common.Errors;

namespace DataAccess.Base;

public sealed class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly DbContext _context;
    private readonly ILogger _logger;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(DbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
        _dbSet = context.Set<T>();
    }
    
    public async Task<Result<T>> GetByIdAsync(int id, bool readOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetByIdAsync called with id: {Id}", id);
        try
        {
            IQueryable<T> query = BuildQuery(predicate: e => e.Id == id, readOnly);
            T? entity = await query.SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (entity != null) return Result.Ok(entity);
            
            _logger.LogInformation("Entity with ID {Id} not found.", id);
            return Result.Fail<T>(new NotFoundError($"Entity with ID {id} not found."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting entity by id: {Id}", id);
            return Result.Fail<T>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }
    
    public async Task<Result<List<T>>> GetByIdsAsync(List<int> ids, bool readOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetByIdsAsync called with ids: {Ids}", ids);
        try
        {
            IQueryable<T> query = BuildQuery(predicate: e => ids.Contains(e.Id), readOnly);

            List<T> entities = await query.ToListAsync(cancellationToken: cancellationToken);

            if (entities.Count != 0) return Result.Ok(entities);
            
            _logger.LogInformation("No entities found with the provided IDs: {Ids}", ids);
            return Result.Fail<List<T>>(new NotFoundError("No entities found with the provided IDs."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting entities by ids: {Ids}", ids);
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

    public async Task<Result<List<T>>> FindAsync(
        Expression<Func<T, bool>> predicate,
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
        _logger.LogInformation("UpdateAsync called for entity with ID {Id}.", entity.Id);
        try
        {
            EntityEntry<T> entry = _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok(entry.Entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating entity with ID {Id}.", entity.Id);
            return Result.Fail<T>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }
    
    public async Task<Result<List<T>>> UpdateAsync(List<T> entities, CancellationToken cancellationToken = default)
    {
        List<int> entityIds = entities.Select(e => e.Id).ToList();
        _logger.LogInformation("UpdateAsync called for entities with IDs {Ids}.", entityIds);
        try
        {
            _dbSet.UpdateRange(entities);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating entities with IDs {Ids}.", entityIds);
            return Result.Fail<List<T>>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }

    public async Task<Result> DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("DeleteAsync called for entity with ID {Id}.", entity.Id);
        try
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting entity with ID {Id}.", entity.Id);
            return Result.Fail(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }
    
    public async Task<Result> DeleteAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        List<T> entityList = entities.ToList();
        List<int> entityIds = entityList.Select(e => e.Id).ToList();
        _logger.LogInformation("DeleteAsync called for entities with IDs {Ids}.", entityIds);

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
                _logger.LogError(ex, "An error occurred while deleting existing entities with IDs {Ids}.", existingEntities.Select(e => e.Id));
                return Result.Fail(new DatabaseError($"An error occurred: {ex.Message}"));
            }

            if (nonexistentEntities.Count == 0)
            {
                return Result.Ok();
            }

            List<int> nonexistentIds = nonexistentEntities.Select(e => e.Id).ToList();
            _logger.LogInformation("Entities with IDs {Ids} do not exist and could not be deleted.", nonexistentIds);
            List<string> errorMessages = 
                nonexistentEntities
                    .Select(e => $"Entity with ID {e.Id} does not exist.")
                    .ToList();
            return Result.Fail(new DatabaseError(string.Join("; ", errorMessages)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting entities with IDs {Ids}.", entityIds);
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
            query.AsNoTrackingWithIdentityResolution();
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return query;
    }
}
