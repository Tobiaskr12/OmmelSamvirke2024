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

public class UploadImagesToAlbumCommandHandler : IRequestHandler<UploadImagesToAlbumCommand, Result<List<Image>>>
{
    private record PreparedImage(Image ImageEntity, byte[] OriginalContent, byte[] DefaultContent, byte[] ThumbnailContent);
    private record UploadAttempt(string BlobName, bool Uploaded);

    private readonly IRepository<Album> _albumRepository;
    private readonly IRepository<BlobStorageFile> _blobRepository;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILoggingHandler _logger;

    public UploadImagesToAlbumCommandHandler(
        IRepository<Album> albumRepository,
        IRepository<BlobStorageFile> blobRepository,
        IImageProcessingService imageProcessingService,
        IBlobStorageService blobStorageService,
        ILoggingHandler logger)
    {
        _albumRepository = albumRepository;
        _blobRepository = blobRepository;
        _imageProcessingService = imageProcessingService;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<Result<List<Image>>> Handle(UploadImagesToAlbumCommand request, CancellationToken cancellationToken)
    {
        if (request.Images.Count == 0)
        {
            return Result.Fail(ErrorMessages.Album_Images_MustProvideAtLeastOne);
        }

        Result<Album> albumResult = await _albumRepository.GetByIdAsync(request.AlbumId, readOnly: false, cancellationToken);
        if (albumResult.IsFailed)
        {
             _logger.LogError(new Exception($"Failed to retrieve album with ID {request.AlbumId} for image upload."), string.Join(", ", albumResult.Errors.Select(e => e.Message)));
             return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        if (albumResult.Value is null)
        {
            _logger.LogInformation($"Attempted to upload images to non-existent album with ID: {request.AlbumId}");
            return Result.Fail(ErrorMessages.GenericNotFound);
        }
        Album targetAlbum = albumResult.Value;

        var imagesToCreate = new List<Image>();
        var preparedImages = new List<PreparedImage>();

        foreach (ImageUploadInput imageInput in request.Images)
        {
            Result<ProcessedImageVersions> processingResult = await _imageProcessingService.GenerateVersionsAsync(
                imageInput.FileContent,
                imageInput.ContentType,
                cancellationToken);

            if (processingResult.IsFailed)
            {
                _logger.LogWarning($"Image processing failed for file {imageInput.OriginalFileName} (Album ID {request.AlbumId}): {processingResult.Errors.FirstOrDefault()?.Message}");
                return Result.Fail(ErrorMessages.Album_ImageProcessingFailed);
            }
            ProcessedImageVersions versions = processingResult.Value;

            string baseFileName = Path.GetFileNameWithoutExtension(imageInput.OriginalFileName);
            string extension = Path.GetExtension(imageInput.OriginalFileName).TrimStart('.');

            var originalBlobMeta = new BlobStorageFile { FileBaseName = $"{baseFileName}_org", FileExtension = extension, ContentType = imageInput.ContentType };
            var defaultBlobMeta = new BlobStorageFile { FileBaseName = $"{baseFileName}_def", FileExtension = extension, ContentType = versions.DefaultVersionContentType };
            var thumbBlobMeta = new BlobStorageFile { FileBaseName = $"{baseFileName}_thumb", FileExtension = extension, ContentType = versions.ThumbnailVersionContentType };

            originalBlobMeta.SetFileSize(imageInput.FileContent.Length);
            defaultBlobMeta.SetFileSize(versions.DefaultVersionContent.Length);
            thumbBlobMeta.SetFileSize(versions.ThumbnailVersionContent.Length);

            ImageMetadataInput? imageMetadata = imageInput.Metadata ?? request.DefaultMetadataForAllImages;
            var image = new Image
            {
                OriginalBlobStorageFile = originalBlobMeta,
                DefaultBlobStorageFile = defaultBlobMeta,
                ThumbnailBlobStorageFile = thumbBlobMeta,
                Album = targetAlbum,
                DateTaken = imageMetadata?.DateTaken,
                Location = imageMetadata?.Location,
                PhotographerName = imageMetadata?.PhotographerName,
                Title = imageMetadata?.Title,
                Description = imageMetadata?.Description
            };
            imagesToCreate.Add(image);

            preparedImages.Add(new PreparedImage(
                image,
                imageInput.FileContent,
                versions.DefaultVersionContent,
                versions.ThumbnailVersionContent
            ));
        }

        Result<Album>? updateAlbumResult;
        var uploadAttempts = new List<UploadAttempt>();
        var createdBlobMetadataIds = new List<int>();

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                targetAlbum.Images.AddRange(imagesToCreate);

                updateAlbumResult = await _albumRepository.UpdateAsync(targetAlbum, cancellationToken);

                if (updateAlbumResult.IsFailed)
                {
                    _logger.LogError(new Exception($"Database update failed for album '{targetAlbum.Name}' (ID: {targetAlbum.Id}) after adding images."), string.Join(", ", updateAlbumResult.Errors.Select(e => e.Message)));
                    return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
                }

                foreach(Image img in imagesToCreate) {
                     if (img.OriginalBlobStorageFile.Id > 0) createdBlobMetadataIds.Add(img.OriginalBlobStorageFile.Id);
                     if (img.DefaultBlobStorageFile.Id > 0) createdBlobMetadataIds.Add(img.DefaultBlobStorageFile.Id);
                     if (img.ThumbnailBlobStorageFile.Id > 0) createdBlobMetadataIds.Add(img.ThumbnailBlobStorageFile.Id);
                }
                createdBlobMetadataIds = createdBlobMetadataIds.Distinct().ToList();

                foreach (PreparedImage prepared in preparedImages)
                {
                    await UploadBlobAndUpdateAttempts(prepared.ImageEntity.OriginalBlobStorageFile, prepared.OriginalContent, uploadAttempts, cancellationToken);
                    await UploadBlobAndUpdateAttempts(prepared.ImageEntity.DefaultBlobStorageFile, prepared.DefaultContent, uploadAttempts, cancellationToken);
                    await UploadBlobAndUpdateAttempts(prepared.ImageEntity.ThumbnailBlobStorageFile, prepared.ThumbnailContent, uploadAttempts, cancellationToken);
                }

                transaction.Complete();
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, $"An error occurred during image upload transaction for album '{targetAlbum.Name}' (ID: {targetAlbum.Id}). Database changes rolled back.");
                 await CleanupPartialUploads(uploadAttempts, cancellationToken);
                 await CleanupCreatedMetadata(createdBlobMetadataIds, cancellationToken);
                 return ex.Message.Contains("Failed to upload blob")
                    ? Result.Fail(ErrorMessages.Album_BlobUploadFailed)
                    : Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
            }
        }

        if (updateAlbumResult.IsFailed)
        {
             _logger.LogError(new Exception("Reached end of UploadImagesToAlbumCommandHandler successfully, but updateAlbumResult was null or failed unexpectedly."), "Unexpected state.");
             return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        _logger.LogInformation($"Successfully uploaded {imagesToCreate.Count} images to Album ID: {targetAlbum.Id}.");
        return Result.Ok(imagesToCreate);
    }

    // Helper Methods
    private async Task UploadBlobAndUpdateAttempts(BlobStorageFile metaData, byte[] content, List<UploadAttempt> attempts, CancellationToken cancellationToken)
    {
        if (metaData.Id == 0) throw new InvalidOperationException("BlobStorageFile ID was not generated before upload attempt.");
        string blobName = $"{metaData.Id}.{metaData.FileExtension}";
        attempts.Add(new UploadAttempt(blobName, false));
        using var contentStream = new MemoryStream(content);
        Result uploadResult = await _blobStorageService.UploadAsync(blobName, contentStream, metaData.ContentType, cancellationToken);
        if (uploadResult.IsFailed) throw new Exception($"Failed to upload blob '{blobName}'.");
        int index = attempts.FindIndex(item => item.BlobName == blobName);
        if (index != -1) { attempts[index] = attempts[index] with { Uploaded = true }; }
    }

    private async Task CleanupPartialUploads(List<UploadAttempt> attempts, CancellationToken cancellationToken)
    {
        List<string> uploadedBlobs = attempts.Where(a => a.Uploaded).Select(a => a.BlobName).ToList();
        if (!uploadedBlobs.Any()) return;
        _logger.LogWarning($"Attempting cleanup for {uploadedBlobs.Count} uploaded blobs due to transaction failure.");
        foreach (string blobName in uploadedBlobs) {
             _logger.LogInformation($"Cleaning up uploaded blob: {blobName}");
             Result deleteResult = await _blobStorageService.DeleteAsync(blobName, cancellationToken);
             if (deleteResult.IsFailed) {
                 _logger.LogError(new Exception($"Failed to clean up blob '{blobName}' after transaction failure."), string.Join(", ", deleteResult.Errors.Select(e => e.Message)));
             }
        }
    }

    private async Task CleanupCreatedMetadata(List<int> metadataIds, CancellationToken cancellationToken)
    {
        if(metadataIds.Count == 0) return;
         _logger.LogWarning($"Attempting to clean up {metadataIds.Count} created BlobStorageFile metadata records due to transaction failure.");
         Result<List<BlobStorageFile>> getResult = await _blobRepository.GetByIdsAsync(metadataIds, readOnly: false, cancellationToken);
         if(getResult.IsSuccess && getResult.Value.Count > 0) {
             Result deleteResult = await _blobRepository.DeleteAsync(getResult.Value, cancellationToken);
             if(deleteResult.IsFailed) {
                 _logger.LogError(new Exception($"Failed to clean up BlobStorageFile metadata records (IDs: {string.Join(", ", metadataIds)}) after transaction failure."), string.Join(", ", deleteResult.Errors.Select(e => e.Message)));
             }
         } else if(getResult.IsFailed) {
            _logger.LogError(new Exception($"Failed to retrieve BlobStorageFile metadata records (IDs: {string.Join(", ", metadataIds)}) for cleanup after transaction failure."), string.Join(", ", getResult.Errors.Select(e => e.Message)));
         }
    }
}
