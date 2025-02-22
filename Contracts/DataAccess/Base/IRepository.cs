using System.Linq.Expressions;
using FluentResults;
using DomainModules.Common;

namespace Contracts.DataAccess.Base;

/// <summary>
/// Represents a generic repository for entities of type <typeparamref name="T"/>.
/// All entities must inherit from <see cref="BaseEntity"/>
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="readOnly">
    /// Determines if the entity should be tracked by the context.
    /// If <c>true</c>, the entity is retrieved without tracking (read-only).
    /// </param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the entity if found, or an error if not.
    /// </returns>
    Task<Result<T>> GetByIdAsync(int id, bool readOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of entities by their identifiers.
    /// </summary>
    /// <param name="ids">The list of identifiers for the entities to retrieve.</param>
    /// <param name="readOnly">
    /// Determines if the entities should be tracked by the context.
    /// If <c>true</c>, the entities are retrieved without tracking (read-only).
    /// </param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A Result of <see cref="List{T}"/> containing the list of entities if found, or an error if not.
    /// </returns>
    Task<Result<List<T>>> GetByIdsAsync(List<int> ids, bool readOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities from the repository.
    /// </summary>
    /// <param name="readOnly">
    /// Determines if the entities should be tracked by the context.
    /// If <c>true</c>, the entities are retrieved without tracking (read-only).
    /// </param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A Result of <see cref="List{T}"/> containing all entities.
    /// </returns>
    Task<Result<List<T>>> GetAllAsync(bool readOnly = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a paginated list of entities.
    /// The results are returned in descending order based on the date they were created.
    /// </summary>
    /// <param name="page">The page number to be used in the query</param>
    /// <param name="pageSize">The number of entities to include per page</param>
    /// <param name="readOnly">
    /// Determines if the entities should be tracked by the context.
    /// If <c>true</c>, the entities are retrieved without tracking (read-only).
    /// </param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="PaginatedResult{T}"/> containing the result entities and the total count of entities
    /// </returns>
    Task<Result<PaginatedResult<T>>> GetPaginatedAsync(int page = 1, int pageSize = 20, bool readOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression to filter the entities.</param>
    /// <param name="readOnly">
    /// Determines if the entities should be tracked by the context.
    /// If <c>true</c>, the entities are retrieved without tracking (read-only).
    /// </param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A Result of <see cref="List{T}"/> containing the matching entities.
    /// </returns>
    Task<Result<List<T>>> FindAsync(
        Expression<Func<T, bool>>? predicate,
        bool readOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the added entity.
    /// </returns>
    Task<Result<T>> AddAsync(
        T entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple new entities to the repository.
    /// </summary>
    /// <param name="entities">The list of entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A Result of <see cref="List{T}"/> containing the added entities.
    /// </returns>
    Task<Result<List<T>>> AddAsync(
        List<T> entities,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository. The entity must not be read-only!
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the updated entity.
    /// </returns>
    Task<Result<T>> UpdateAsync(
        T entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple existing entities in the repository. The entities must not be read-only!
    /// </summary>
    /// <param name="entities">The list of entities to update.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A Result of <see cref="List{T}"/> containing the updated entities.
    /// </returns>
    Task<Result<List<T>>> UpdateAsync(
        List<T> entities,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the repository. The entity must not be read-only!
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success or failure.
    /// </returns>
    Task<Result> DeleteAsync(
        T entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities from the repository. The entities must not be read-only!
    /// </summary>
    /// <param name="entities">The collection of entities to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success or failure.
    /// </returns>
    Task<Result> DeleteAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default);
}
