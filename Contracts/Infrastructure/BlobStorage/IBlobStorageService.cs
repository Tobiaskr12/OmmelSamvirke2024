using FluentResults;

namespace Contracts.Infrastructure.BlobStorage; // Or your appropriate namespace

/// <summary>
/// Defines operations for interacting with a configured blob storage provider.
/// Assumes a single container is configured per environment.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a blob to the configured container.
    /// </summary>
    /// <param name="blobName">The desired name for the blob within the container (e.g., "{id}.{extension}").</param>
    /// <param name="content">The stream containing the binary content of the blob.</param>
    /// <param name="contentType">The MIME type of the content (e.g., "image/jpeg").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    Task<Result> UploadAsync(
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a blob from the configured container.
    /// </summary>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure. Success indicates the blob either was deleted or did not exist.</returns>
    Task<Result> DeleteAsync(
        string blobName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the configured base URL for constructing public blob links.
    /// Example: "https://youraccount.blob.core.windows.net/yourcontainer/"
    /// </summary>
    /// <returns>The base URL string, including the container name and trailing slash.</returns>
    string GetPublicBlobBaseUrl();
}
