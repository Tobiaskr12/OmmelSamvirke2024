using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Contracts.Infrastructure.BlobStorage;
using FluentResults;
using Infrastructure.Errors;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.BlobStorage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly string _publicBaseUrl;
    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    public AzureBlobStorageService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration["AzureBlobStorageConnectionString"]
                                  ?? throw new InvalidOperationException("Missing AzureBlobStorageConnectionString in configuration");
        string containerName = configuration["AzureBlobStorageContainerName"]
                               ?? throw new InvalidOperationException("Missing AzureBlobStorageContainerName in configuration");

        // Initialize the client for the container
        _blobContainerClient = new BlobContainerClient(connectionString, containerName);

        // Construct and store the public base URL
        _publicBaseUrl = $"https://ommelsamvirke.blob.core.windows.net/{containerName}/";
    }

    public async Task<Result> UploadAsync(
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);
            if (content.CanSeek) content.Position = 0;

            // Retry loop
            int attempt = 0;
            while (true) 
            {
                try
                {
                    // Reset stream position before each upload attempt if possible
                    if (content.CanSeek) content.Position = 0;

                    var uploadOptions = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                        TransferOptions = new StorageTransferOptions
                        {
                            MaximumTransferSize = 4 * 1024 * 1024,
                            InitialTransferSize = 4 * 1024 * 1024
                        }
                    };

                    await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

                    // Upload succeeded
                    return Result.Ok();
                }
                catch (RequestFailedException) when (++attempt < MaxRetryAttempts)
                {
                    await Task.Delay(RetryDelay, cancellationToken);
                }
                catch (RequestFailedException ex)
                {
                    return Result.Fail($"{ErrorMessages.BlobStorage_Upload_Failed}: Attempt {attempt} failed with status {ex.Status}. ErrorCode: {ex.ErrorCode}. Message: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            return Result.Fail($"{ErrorMessages.BlobStorage_Upload_Failed}: An unexpected error occurred. {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
            
            return Result.Ok();
        }
        catch (RequestFailedException ex)
        {
            return Result.Fail($"{ErrorMessages.BlobStorage_Delete_Failed}: Status {ex.Status}. ErrorCode: {ex.ErrorCode}. Message: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"{ErrorMessages.BlobStorage_Delete_Failed}: An unexpected error occurred. {ex.Message}");
        }
    }

    public string GetPublicBlobBaseUrl()
    {
        return _publicBaseUrl;
    }
}
