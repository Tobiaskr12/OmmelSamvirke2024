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

public class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand, Result>
{
    private readonly IRepository<Image> _imageRepository;
    private readonly IRepository<BlobStorageFile> _blobRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILoggingHandler _logger;

    public DeleteImageCommandHandler(
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

    public async Task<Result> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
    {
        // Get Image with related Blob Metadata
        Result<List<Image>> findResult = await _imageRepository.FindAsync(
            predicate: img => img.Id == request.ImageId,
            readOnly: false,
            cancellationToken: cancellationToken
        );

        if (findResult.IsFailed)
        {
             _logger.LogError(new Exception($"Failed to retrieve image with ID {request.ImageId} for deletion."), string.Join(", ", findResult.Errors.Select(e => e.Message)));
             return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        if (findResult.Value.Count == 0)
        {
             _logger.LogInformation($"Image with ID {request.ImageId} not found for deletion.");
             return Result.Fail(ErrorMessages.GenericNotFound);
        }

        Image imageToDelete = findResult.Value.First();
        List<BlobStorageFile> blobsToDelete = [];
        List<string> blobNamesToDelete = [];

        // Collect blobs and their names
        blobsToDelete.Add(imageToDelete.OriginalBlobStorageFile);
        blobNamesToDelete.Add($"{imageToDelete.OriginalBlobStorageFile.Id}.{imageToDelete.OriginalBlobStorageFile.FileExtension}");
        blobsToDelete.Add(imageToDelete.DefaultBlobStorageFile);
        blobNamesToDelete.Add($"{imageToDelete.DefaultBlobStorageFile.Id}.{imageToDelete.DefaultBlobStorageFile.FileExtension}");

        blobsToDelete.Add(imageToDelete.ThumbnailBlobStorageFile);
        blobNamesToDelete.Add($"{imageToDelete.ThumbnailBlobStorageFile.Id}.{imageToDelete.ThumbnailBlobStorageFile.FileExtension}");

        // Delete Blobs from Storage
        foreach(string blobName in blobNamesToDelete)
        {
            Result deleteBlobResult = await _blobStorageService.DeleteAsync(blobName, cancellationToken);
            if(deleteBlobResult.IsFailed)
            {
                _logger.LogWarning($"Failed to delete blob '{blobName}' from storage for Image ID {imageToDelete.Id}. Errors: {string.Join(", ", deleteBlobResult.Errors.Select(e => e.Message))}");
            }
        }
        
        // Delete the Image record
        Result deleteImageResult = await _imageRepository.DeleteAsync(imageToDelete, cancellationToken);
        if(deleteImageResult.IsFailed)
        {
            _logger.LogError(new Exception($"Database delete failed for Image ID {imageToDelete.Id}."), string.Join(", ", deleteImageResult.Errors.Select(e => e.Message)));
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        // Delete the metadata records
        if (blobsToDelete.Count > 0)
        {
            Result deleteBlobMetaResult = await _blobRepository.DeleteAsync(blobsToDelete, cancellationToken);
            if(deleteBlobMetaResult.IsFailed)
            {
                 _logger.LogError(new Exception($"Database delete failed for BlobStorageFile metadata associated with Image ID {imageToDelete.Id}."), string.Join(", ", deleteBlobMetaResult.Errors.Select(e => e.Message)));
                 return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
            }
        }
        
        return Result.Ok();
    }
}
