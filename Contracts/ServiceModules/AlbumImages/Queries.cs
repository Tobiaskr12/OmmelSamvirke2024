using Contracts.DataAccess;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.AlbumImages;

public record GetAlbumsPaginatedQuery(
    int Page = 1,
    int PageSize = 20,
    SortDirection SortBy = SortDirection.NewestFirst
) : IRequest<Result<PaginatedResult<AlbumSummaryDto>>>;


public record GetAlbumImagesPaginatedQuery(
    int AlbumId,
    int Page = 1,
    int PageSize = 20,
    SortDirection SortBy = SortDirection.NewestFirst
) : IRequest<Result<PaginatedResult<AlbumImageDto>>>;