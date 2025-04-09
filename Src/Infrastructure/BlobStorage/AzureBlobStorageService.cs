using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Contracts.Infrastructure.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using Infrastructure.Errors;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.BlobStorage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _blobContainerClient;
    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    public AzureBlobStorageService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration["AzureBlobStorageConnectionString"] 
                                  ?? throw new Exception("Missing AzureBlobStorageConnectionString in configuration");
        string containerName = configuration["AzureBlobStorageContainerName"] 
                               ?? throw new Exception("Missing AzureBlobStorageContainerName in configuration");

        _blobContainerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<Result> UploadBlobAsync(
        BlobStorageFile blobStorageFile,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(blobStorageFile);
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            // Get the computed blob name "<FileBaseName>###<BlobGuid>.<FileExtension>"
            string blobName = blobStorageFile.FileName;
            BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);

            // Ensure the stream is at the beginning
            if (content.CanSeek) content.Position = 0;

            int attempt = 0;
            while (true)
            {
                try
                {
                    // Reset stream position before each upload attempt if possible
                    if (content.CanSeek) content.Position = 0;

                    var uploadOptions = new BlobUploadOptions
                    {
                        TransferOptions = new StorageTransferOptions
                        {
                            MaximumTransferSize = 4 * 1024 * 1024, // 4 MB chunks
                            InitialTransferSize = 4 * 1024 * 1024
                        }
                    };

                    await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

                    // Update file size from the stream
                    blobStorageFile.SetFileSize(content.Length);
                    return Result.Ok();
                }
                catch (RequestFailedException) when (++attempt < MaxRetryAttempts)
                {
                    await Task.Delay(RetryDelay, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            return Result.Fail(ErrorMessages.BlobStorage_Upload_Failed + ": " + ex.Message);
        }
    }

    public async Task<Result<BlobStorageFile>> DownloadBlobAsync(
        BlobStorageFile blobStorageFile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(blobStorageFile);

        try
        {
            string blobGuidString = blobStorageFile.BlobGuid.ToString();

            // Iterate through blobs in the container to find the matching naming pattern
            await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync(cancellationToken: cancellationToken))
            {
                // Expected format: "<FileBaseName>###<BlobGuid>.<FileExtension>"
                if (blobItem.Name.Contains($"###{blobGuidString}."))
                {
                    BlobClient blobClient = _blobContainerClient.GetBlobClient(blobItem.Name);
                    var memoryStream = new MemoryStream();

                    int attempt = 0;
                    while (true)
                    {
                        try
                        {
                            await blobClient.DownloadToAsync(memoryStream, cancellationToken);
                            break;
                        }
                        catch (RequestFailedException) when (++attempt < MaxRetryAttempts)
                        {
                            await Task.Delay(RetryDelay, cancellationToken);
                        }
                    }

                    // Reset stream position and assign it to the entity
                    memoryStream.Position = 0;
                    blobStorageFile.FileContent = memoryStream;
                    blobStorageFile.SetFileSize(memoryStream.Length);

                    return Result.Ok(blobStorageFile);
                }
            }

            return Result.Fail<BlobStorageFile>(ErrorMessages.BlobStorage_BlobNotFound);
        }
        catch (Exception ex)
        {
            return Result.Fail<BlobStorageFile>(ErrorMessages.BlobStorage_Download_Failed + ": " + ex.Message);
        }
    }
}
