using DomainModules.BlobStorage.Entities;
using FluentResults;

namespace Contracts.Infrastructure.BlobStorage;

public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a blob to a third party Blob Storage service.
    /// </summary>
    /// <param name="blobStorageFile">The BlobStorageFile metadata instance.</param>
    /// <param name="content">The stream containing the binary content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    Task<Result> UploadBlobAsync(BlobStorageFile blobStorageFile, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a blob from a third party Blob Storage service based on the provided BlobStorageFile's BlobGuid
    /// </summary>
    /// <param name="blobStorageFile">Entity containing data about the blob. Will also contain the downloaded data</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Result{BlobStorageFile}"/> containing the file metadata (and content) if found.</returns>
    Task<Result<BlobStorageFile>> DownloadBlobAsync(BlobStorageFile  blobStorageFile, CancellationToken cancellationToken = default);
}
