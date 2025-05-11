using FluentResults;

namespace Contracts.ServiceModules.AlbumImages;

public record ProcessedImageVersions(
    byte[] DefaultVersionContent,
    string DefaultVersionContentType,
    byte[] ThumbnailVersionContent,
    string ThumbnailVersionContentType
);

public interface IImageProcessingService
{
    /// <summary>
    /// Takes original image data and generates default and thumbnail versions.
    /// </summary>
    /// <param name="originalImageContent">Content of the original image.</param>
    /// <param name="originalContentType">MIME type of the original image.</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing the processed versions.</returns>
    Task<Result<ProcessedImageVersions>> GenerateVersionsAsync(
        byte[] originalImageContent, 
        string originalContentType,
        CancellationToken cancellationToken = default
    );
}