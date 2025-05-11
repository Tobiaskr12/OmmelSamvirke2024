using Contracts.DataAccess.Base;
using Contracts.Infrastructure.BlobStorage;
using Contracts.ServiceModules.BlobStorage;
using Contracts.SupportModules.Logging;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.BlobStorage.Commands;

public class DeleteBlobCommandHandler : IRequestHandler<DeleteBlobCommand, Result>
{
    private readonly IRepository<BlobStorageFile> _blobRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILoggingHandler _logger;

    public DeleteBlobCommandHandler(
        IRepository<BlobStorageFile> blobRepository,
        IBlobStorageService blobStorageService,
        ILoggingHandler logger)
    {
        _blobRepository = blobRepository;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteBlobCommand request, CancellationToken cancellationToken)
    {
        Result<BlobStorageFile> getResult = await _blobRepository.GetByIdAsync(request.BlobMetadataId, readOnly: false, cancellationToken);

        if (getResult.IsFailed)
        {
             _logger.LogError(new Exception($"Failed to retrieve blob metadata with ID {request.BlobMetadataId} for deletion."), string.Join(", ", getResult.Errors.Select(e => e.Message)));
             return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt).WithErrors(getResult.Errors);
        }
        if (getResult.Value is null)
        {
             _logger.LogInformation($"Blob metadata with ID {request.BlobMetadataId} not found for deletion.");
             return Result.Fail(ErrorMessages.GenericNotFound); 
        }

        BlobStorageFile blobMetadata = getResult.Value;
        string blobName = $"{blobMetadata.FileBaseName}-{blobMetadata.Id}.{blobMetadata.FileExtension}";

        // Delete Blob from Storage
        Result deleteBlobResult = await _blobStorageService.DeleteAsync(blobName, cancellationToken);
        if (deleteBlobResult.IsFailed)
        {
             _logger.LogWarning($"Failed to delete blob '{blobName}' from storage. Errors: {string.Join(", ", deleteBlobResult.Errors.Select(e => e.Message))}");
             return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        // Delete Metadata from Database
        Result deleteMetaResult = await _blobRepository.DeleteAsync(blobMetadata, cancellationToken);
        if (deleteMetaResult.IsFailed)
        {
            _logger.LogError(new Exception($"Database delete failed for blob metadata ID {blobMetadata.Id}."), string.Join(", ", deleteMetaResult.Errors.Select(e => e.Message)));
            // Even if blob deletion succeeded, DB failure is a problem
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        
        return Result.Ok();
    }
}
