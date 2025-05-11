using Contracts.DataAccess;
using Contracts.DataAccess.Base;
using Contracts.Infrastructure.BlobStorage;
using Contracts.ServiceModules.AlbumImages;
using Contracts.SupportModules.Logging;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;
using DomainModules.BlobStorage.Entities;

namespace ServiceModules.ImageAlbums.Queries;

public class GetAlbumsPaginatedQueryHandler : IRequestHandler<GetAlbumsPaginatedQuery, Result<PaginatedResult<AlbumSummaryDto>>>
{
    private readonly IRepository<Album> _albumRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILoggingHandler _logger;

    public GetAlbumsPaginatedQueryHandler(
        IRepository<Album> albumRepository,
        IBlobStorageService blobStorageService,
        ILoggingHandler logger)
    {
        _albumRepository = albumRepository;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<AlbumSummaryDto>>> Handle(GetAlbumsPaginatedQuery request, CancellationToken cancellationToken)
    {
        Result<PaginatedResult<Album>> repoResult = await _albumRepository.GetPaginatedAsync(
            page: request.Page,
            pageSize: request.PageSize,
            readOnly: true,
            sortDirection: request.SortBy,
            cancellationToken: cancellationToken);

        if (repoResult.IsFailed)
        {
            _logger.LogWarning($"Failed to retrieve paginated albums: {string.Join(", ", repoResult.Errors.Select(e => e.Message))}");
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        PaginatedResult<Album> albums = repoResult.Value;
        string baseUrl = _blobStorageService.GetPublicBlobBaseUrl();

        List<AlbumSummaryDto> dtos = albums.Items.Select(album =>
        {
            string? thumbnailUrl = null;
            if (album.CoverImage?.ThumbnailBlobStorageFile is not null)
            {
                BlobStorageFile thumbMeta = album.CoverImage.ThumbnailBlobStorageFile;
                string blobName = $"{thumbMeta.FileBaseName}-{thumbMeta.Id}.{thumbMeta.FileExtension}";
                thumbnailUrl = baseUrl + blobName;
            }

            return new AlbumSummaryDto(
                album.Id,
                album.Name,
                album.Description,
                thumbnailUrl,
                album.Images.Count,
                album.DateCreated ?? default
            );
        }).ToList();

        var paginatedDtoResult = new PaginatedResult<AlbumSummaryDto>
        {
            Items = dtos,
            ItemsCount = albums.ItemsCount,
            Page = albums.Page,
            PageSize = albums.PageSize,
            PageCount = albums.PageCount
        };
        
        return Result.Ok(paginatedDtoResult);
    }
}
