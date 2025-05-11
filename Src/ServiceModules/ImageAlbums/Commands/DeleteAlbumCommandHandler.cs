using Contracts.DataAccess.Base;
using Contracts.Infrastructure.BlobStorage;
using Contracts.ServiceModules.AlbumImages;
using Contracts.SupportModules.Logging;
using DomainModules.BlobStorage.Entities;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;
using System.Transactions;

namespace ServiceModules.ImageAlbums.Commands;

public class DeleteAlbumCommandHandler : IRequestHandler<DeleteAlbumCommand, Result>
{
    private readonly IRepository<Album> _albumRepository;
    private readonly IRepository<Image> _imageRepository;
    private readonly IRepository<BlobStorageFile> _blobRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILoggingHandler _logger;

    public DeleteAlbumCommandHandler(
        IRepository<Album> albumRepository,
        IRepository<Image> imageRepository,
        IRepository<BlobStorageFile> blobRepository,
        IBlobStorageService blobStorageService,
        ILoggingHandler logger)
    {
        _albumRepository = albumRepository;
        _imageRepository = imageRepository;
        _blobRepository = blobRepository;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteAlbumCommand request, CancellationToken cancellationToken)
    {
        Result<Album> albumResult = await _albumRepository.GetByIdAsync(request.AlbumId, readOnly: false, cancellationToken);

        if (albumResult.IsFailed)
        {
            _logger.LogError(new Exception($"Failed to retrieve album with ID {request.AlbumId} for deletion."), string.Join(", ", albumResult.Errors.Select(e => e.Message)));
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        if (albumResult.Value is null)
        {
            _logger.LogInformation($"Attempted to delete non-existent album with ID: {request.AlbumId}");
            return Result.Fail(ErrorMessages.GenericNotFound);
        }

        Album albumToDelete = albumResult.Value;
        List<Image> imagesToDelete = albumToDelete.Images.ToList();
        List<BlobStorageFile> blobsToDelete = [];
        List<string> blobNamesToDelete = [];

        foreach (Image image in imagesToDelete)
        {
            blobsToDelete.Add(image.OriginalBlobStorageFile);
            blobNamesToDelete.Add($"{image.OriginalBlobStorageFile.Id}.{image.OriginalBlobStorageFile.FileExtension}");
            blobsToDelete.Add(image.DefaultBlobStorageFile);
            blobNamesToDelete.Add($"{image.DefaultBlobStorageFile.Id}.{image.DefaultBlobStorageFile.FileExtension}");
            blobsToDelete.Add(image.ThumbnailBlobStorageFile);
            blobNamesToDelete.Add($"{image.ThumbnailBlobStorageFile.Id}.{image.ThumbnailBlobStorageFile.FileExtension}");
        }
        blobsToDelete = blobsToDelete.DistinctBy(b => b.Id).ToList();
        blobNamesToDelete = blobNamesToDelete.Distinct().ToList();

        bool blobDeletionWarning = false;
        foreach (string blobName in blobNamesToDelete)
        {
            Result deleteBlobResult = await _blobStorageService.DeleteAsync(blobName, cancellationToken);
            if (deleteBlobResult.IsFailed)
            {
                blobDeletionWarning = true;
                _logger.LogWarning($"Failed to delete blob '{blobName}' from storage for Album ID {albumToDelete.Id}. Errors: {string.Join(", ", deleteBlobResult.Errors.Select(e => e.Message))}");
            }
        }

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                if (imagesToDelete.Count > 0)
                {
                    Result deleteImagesResult = await _imageRepository.DeleteAsync(imagesToDelete, cancellationToken);
                    if (deleteImagesResult.IsFailed)
                    {
                        _logger.LogError(new Exception($"Database delete failed for Image records for Album ID {albumToDelete.Id}."), string.Join(", ", deleteImagesResult.Errors.Select(e => e.Message)));
                        return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
                    }
                }
                
                if (blobsToDelete.Count > 0)
                {
                    Result deleteBlobMetaResult = await _blobRepository.DeleteAsync(blobsToDelete, cancellationToken);
                    if (deleteBlobMetaResult.IsFailed)
                    {
                        _logger.LogError(new Exception($"Database delete failed for BlobStorageFile metadata for Album ID {albumToDelete.Id}."), string.Join(", ", deleteBlobMetaResult.Errors.Select(e => e.Message)));
                        return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
                    }
                }
                
                // Delete album
                Result deleteAlbumResult = await _albumRepository.DeleteAsync(albumToDelete, cancellationToken);
                if (deleteAlbumResult.IsFailed)
                {
                    _logger.LogError(new Exception($"Database delete failed for Album ID {albumToDelete.Id}."), string.Join(", ", deleteAlbumResult.Errors.Select(e => e.Message)));
                    return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
                }

                transaction.Complete();
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, $"An unexpected error occurred during database deletion transaction for Album ID {albumToDelete.Id}.");
                 return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
            }
        }

        _logger.LogInformation($"Successfully deleted Album ID {albumToDelete.Id}" + (blobDeletionWarning ? " (with blob storage deletion warnings)." : "."));
        return Result.Ok();
    }
}
