using Contracts.DataAccess.Base;
using Contracts.ServiceModules.AlbumImages;
using Contracts.SupportModules.Logging;
using DomainModules.BlobStorage.Entities;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;
using IBlobStorageService = Contracts.Infrastructure.BlobStorage.IBlobStorageService;

namespace ServiceModules.ImageAlbums.Commands;

public class DeleteImagesCommandHandler : IRequestHandler<DeleteImagesCommand, Result>
{
    private readonly IRepository<Image> _imageRepository;
    private readonly IRepository<BlobStorageFile> _blobRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILoggingHandler _logger;

    public DeleteImagesCommandHandler(
        IRepository<Image> imageRepository,
        IRepository<BlobStorageFile> blobRepository,
        IBlobStorageService blobStorageService,
        ILoggingHandler logger)
    {
        _imageRepository = imageRepository;
        _blobRepository = blobRepository;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteImagesCommand request, CancellationToken cancellationToken)
    {
        if (request.ImageIds.Count == 0)
        {
            return Result.Ok();
        }

        // Get Images with related Blob Metadata
        Result<List<Image>> findResult = await _imageRepository.FindAsync(
            predicate: img => request.ImageIds.Contains(img.Id),
            readOnly: false,
            cancellationToken: cancellationToken);

        if (findResult.IsFailed)
        {
             _logger.LogError(new Exception($"Failed to retrieve images for deletion. IDs: [{string.Join(", ", request.ImageIds)}]."), string.Join(", ", findResult.Errors.Select(e => e.Message)));
             return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        List<Image> imagesToDelete = findResult.Value;
        if (!imagesToDelete.Any())
        {
             _logger.LogInformation($"No images found for deletion matching IDs: [{string.Join(", ", request.ImageIds)}].");
             return Result.Ok();
        }

        List<BlobStorageFile> allBlobsToDelete = [];
        List<string> allBlobNamesToDelete = [];

        // Collect all blobs and their names
        foreach (Image image in imagesToDelete)
        {
            allBlobsToDelete.Add(image.OriginalBlobStorageFile);
            allBlobNamesToDelete.Add($"{image.OriginalBlobStorageFile.Id}.{image.OriginalBlobStorageFile.FileExtension}");
            allBlobsToDelete.Add(image.DefaultBlobStorageFile);
            allBlobNamesToDelete.Add($"{image.DefaultBlobStorageFile.Id}.{image.DefaultBlobStorageFile.FileExtension}");
            allBlobsToDelete.Add(image.ThumbnailBlobStorageFile);
            allBlobNamesToDelete.Add($"{image.ThumbnailBlobStorageFile.Id}.{image.ThumbnailBlobStorageFile.FileExtension}");
        }

        // Delete Blobs from Storage
        foreach(string blobName in allBlobNamesToDelete)
        {
            Result deleteBlobResult = await _blobStorageService.DeleteAsync(blobName, cancellationToken);
            if(deleteBlobResult.IsFailed)
            {
                _logger.LogWarning($"Failed to delete blob '{blobName}' from storage during bulk image delete. Errors: {string.Join(", ", deleteBlobResult.Errors.Select(e => e.Message))}");
            }
        }
        
        // Delete the Image records
        Result deleteImagesResult = await _imageRepository.DeleteAsync(imagesToDelete, cancellationToken);
        if(deleteImagesResult.IsFailed)
        {
            _logger.LogError(new Exception($"Database delete failed for Images during bulk delete."), string.Join(", ", deleteImagesResult.Errors.Select(e => e.Message)));
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        // Delete Blob Metadata Records
        if (allBlobsToDelete.Count > 0)
        {
            List<BlobStorageFile> uniqueBlobsToDelete = allBlobsToDelete.DistinctBy(b => b.Id).ToList();
            Result deleteBlobMetaResult = await _blobRepository.DeleteAsync(uniqueBlobsToDelete, cancellationToken);
            if(deleteBlobMetaResult.IsFailed)
            {
                 _logger.LogError(new Exception($"Database delete failed for BlobStorageFile metadata during bulk image delete."), string.Join(", ", deleteBlobMetaResult.Errors.Select(e => e.Message)));
                 return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
            }
        }

        return Result.Ok();
    }
}
