using Contracts.DataAccess;
using Contracts.DataAccess.Base;
using Contracts.Infrastructure.BlobStorage;
using Contracts.ServiceModules.AlbumImages;
using Contracts.SupportModules.Logging;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;
using System.Linq.Expressions;
using DomainModules.BlobStorage.Entities;

namespace ServiceModules.ImageAlbums.Queries;

public class GetAlbumImagesPaginatedQueryHandler : IRequestHandler<GetAlbumImagesPaginatedQuery, Result<PaginatedResult<AlbumImageDto>>>
{
    private readonly IRepository<Image> _imageRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILoggingHandler _logger;

    public GetAlbumImagesPaginatedQueryHandler(
        IRepository<Image> imageRepository,
        IBlobStorageService blobStorageService,
        ILoggingHandler logger)
    {
        _imageRepository = imageRepository;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }
    
    public async Task<Result<PaginatedResult<AlbumImageDto>>> Handle(GetAlbumImagesPaginatedQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Image, bool>> filter = img => img.Album.Id == request.AlbumId;
        
        Result<PaginatedResult<Image>> repoResult = await _imageRepository.GetPaginatedAsync(
            predicate: filter,
            page: request.Page,
            pageSize: request.PageSize,
            readOnly: true,
            sortDirection: request.SortBy,
            cancellationToken: cancellationToken);

        if (repoResult.IsFailed)
        {
            _logger.LogWarning($"Failed to retrieve paginated images for Album ID {request.AlbumId}: {string.Join(", ", repoResult.Errors.Select(e => e.Message))}");
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        PaginatedResult<Image> images = repoResult.Value;
        string baseUrl = _blobStorageService.GetPublicBlobBaseUrl();
        
        List<AlbumImageDto> imageDtos = images.Items.Select(img =>
        {
            // Construct Thumbnail URL
            BlobStorageFile thumbMeta = img.ThumbnailBlobStorageFile;
            string thumbBlobName = $"{thumbMeta.FileBaseName}-{thumbMeta.Id}.{thumbMeta.FileExtension}";
            string thumbnailUrl = baseUrl + thumbBlobName;

            // Construct Default URL
            BlobStorageFile defaultMeta = img.DefaultBlobStorageFile;
            string defaultBlobName = $"{defaultMeta.FileBaseName}-{defaultMeta.Id}.{defaultMeta.FileExtension}";
            string defaultUrl = baseUrl + defaultBlobName;

            return new AlbumImageDto(
                img.Id,
                thumbnailUrl,
                defaultUrl,
                img.Title,
                img.Description,
                img.DateCreated ?? default
            );
        }).ToList();
        
        var paginatedDtoResult = new PaginatedResult<AlbumImageDto>
        {
            Items = imageDtos,
            Page = request.Page,
            PageSize = request.PageSize,
            ItemsCount = images.ItemsCount,
            PageCount = images.PageCount
        };

        return Result.Ok(paginatedDtoResult);
    }
}
