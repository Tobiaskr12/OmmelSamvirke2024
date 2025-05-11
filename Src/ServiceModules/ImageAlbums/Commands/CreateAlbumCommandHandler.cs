using System.Transactions;
using Contracts.DataAccess.Base;
using Contracts.Infrastructure.BlobStorage;
using Contracts.ServiceModules.AlbumImages;
using DomainModules.BlobStorage.Entities;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.ImageAlbums.Commands;

public sealed class CreateAlbumCommandHandler : IRequestHandler<CreateAlbumCommand, Result<Album>>
{
    private record PreparedImage(Image ImageEntity, byte[] OriginalContent, byte[] DefaultContent, byte[] ThumbnailContent);

    private readonly IRepository<Album> _albumRepository;
    private readonly IImageProcessingService _imageProcessing;
    private readonly IBlobStorageService _blobStorage;

    public CreateAlbumCommandHandler(
        IRepository<Album> albumRepository,
        IImageProcessingService imageProcessing,
        IBlobStorageService blobStorage)
    {
        _albumRepository = albumRepository;
        _imageProcessing = imageProcessing;
        _blobStorage = blobStorage;
    }

    public async Task<Result<Album>> Handle(CreateAlbumCommand request, CancellationToken token)
    {
        if (request.Images.Count == 0) return Result.Fail(ErrorMessages.Album_Images_InvalidSize);

        var album = new Album
        {
            Name = request.Name, 
            Description = request.Description, 
            Images = []
        };
        var preparedImages = new List<PreparedImage>();

        // Build Image entities and process content
        foreach (ImageUploadInput input in request.Images)
        {
            Result<ProcessedImageVersions> processedImages = await _imageProcessing.GenerateVersionsAsync(
                input.FileContent, 
                input.ContentType, 
                token);

            if (processedImages.IsFailed) return Result.Fail(ErrorMessages.Album_ImageProcessingFailed);

            (byte[] defaultBytes,  string defType) = (processedImages.Value.DefaultVersionContent, processedImages.Value.DefaultVersionContentType);
            (byte[] thumbnailBytes, string thumbType) = (processedImages.Value.ThumbnailVersionContent, processedImages.Value.ThumbnailVersionContentType);

            string baseName = Path.GetFileNameWithoutExtension(input.OriginalFileName);
            string extension = Path.GetExtension(input.OriginalFileName).TrimStart('.');

            BlobStorageFile originalBlob = BuildMeta($"{baseName}_org", extension, input.ContentType, input.FileContent.Length);
            BlobStorageFile defaultBlob = BuildMeta($"{baseName}_def", extension, defType, defaultBytes.Length);
            BlobStorageFile thumbnailBlob = BuildMeta($"{baseName}_thumb",extension, thumbType, thumbnailBytes.Length);

            ImageMetadataInput? meta = input.Metadata ?? request.DefaultMetadataForAllImages;

            var image = new Image
            {
                OriginalBlobStorageFile = originalBlob,
                DefaultBlobStorageFile = defaultBlob,
                ThumbnailBlobStorageFile = thumbnailBlob,
                Album = album,
                DateTaken = meta?.DateTaken,
                Location = meta?.Location,
                PhotographerName = meta?.PhotographerName,
                Title = meta?.Title,
                Description = meta?.Description
            };

            album.Images.Add(image);
            preparedImages.Add(new PreparedImage(image, input.FileContent, defaultBytes, thumbnailBytes));
        }

        // Insert Album + Images and upload blobs
        Result<Album> addResult;
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            addResult = await _albumRepository.AddAsync(album, token);
            if (addResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);

            foreach (PreparedImage image in preparedImages)
            {
                await UploadBlob(image.ImageEntity.OriginalBlobStorageFile, image.OriginalContent, token);
                await UploadBlob(image.ImageEntity.DefaultBlobStorageFile, image.DefaultContent, token);
                await UploadBlob(image.ImageEntity.ThumbnailBlobStorageFile, image.ThumbnailContent, token);
            }

            scope.Complete();
        }

        // Set cover image
        Album persisted = addResult.Value;
        persisted.CoverImage = persisted.Images.First();

        Result<Album> updateResult = await _albumRepository.UpdateAsync(persisted, token);
        return updateResult.IsFailed 
            ? Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt)
            : Result.Ok(updateResult.Value);

        // Local helpers
        BlobStorageFile BuildMeta(string name, string extension, string contentType, long fileSize)
        {
            var blob = new BlobStorageFile
            {
                FileBaseName = name,
                FileExtension = extension, 
                ContentType = contentType
            };
            blob.SetFileSize(fileSize);
            return blob;
        }

        async Task UploadBlob(BlobStorageFile metadata, byte[] bytes, CancellationToken cancellationToken)
        {
            string blobName = $"{metadata.FileBaseName}-{metadata.Id}.{metadata.FileExtension}";

            await using var ms = new MemoryStream(bytes);
            Result uploadResult = await _blobStorage.UploadAsync(blobName, ms, metadata.ContentType, cancellationToken);
            if (uploadResult.IsFailed) throw new Exception($"Failed to upload {blobName}");
        }
    }
}
