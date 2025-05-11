using Contracts.DataAccess.Base;
using Contracts.Infrastructure.BlobStorage;
using Contracts.ServiceModules.BlobStorage;
using Contracts.SupportModules.Logging;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;
using System.Transactions;
using FluentValidation;
using FluentValidation.Results;
using ServiceModules.Errors;

namespace ServiceModules.BlobStorage.Commands;

public class CreateAndUploadBlobCommandHandler : IRequestHandler<CreateAndUploadBlobCommand, Result<BlobStorageFile>>
{
    private readonly IRepository<BlobStorageFile> _blobRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILoggingHandler _logger;
    private readonly IValidator<BlobStorageFile> _validator;

    public CreateAndUploadBlobCommandHandler(
        IRepository<BlobStorageFile> blobRepository,
        IBlobStorageService blobStorageService,
        ILoggingHandler logger,
        IValidator<BlobStorageFile> validator)
    {
        _blobRepository = blobRepository;
        _blobStorageService = blobStorageService;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<BlobStorageFile>> Handle(CreateAndUploadBlobCommand request, CancellationToken cancellationToken)
    {
        // Prepare metadata entity
        var blobMetadata = new BlobStorageFile
        {
            FileBaseName = request.FileBaseName,
            FileExtension = request.FileExtension,
            ContentType = request.ContentType
        };
        if(request.FileContent.CanSeek)
        {
            blobMetadata.SetFileSize(request.FileContent.Length);
        }

        ValidationResult? validationResult = await _validator.ValidateAsync(blobMetadata, cancellationToken);
        if (!validationResult.IsValid) return Result.Fail(string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage)));
        
        string blobName;
        Result<BlobStorageFile>? addResult;

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                addResult = await _blobRepository.AddAsync(blobMetadata, cancellationToken);
                if (addResult.IsFailed)
                {
                    _logger.LogError(new Exception($"Database save failed for new blob metadata '{request.FileBaseName}.{request.FileExtension}'."), string.Join(", ", addResult.Errors.Select(e => e.Message)));
                    return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
                }

                blobMetadata = addResult.Value;
                blobName = $"{blobMetadata.FileBaseName}-{blobMetadata.Id}.{blobMetadata.FileExtension}";

                //Upload Blob
                Result uploadResult = await _blobStorageService.UploadAsync(
                    blobName,
                    request.FileContent,
                    blobMetadata.ContentType,
                    cancellationToken);

                if (uploadResult.IsFailed)
                {
                    _logger.LogError(new Exception($"Blob storage upload failed for blob '{blobName}'."), string.Join(", ", uploadResult.Errors.Select(e => e.Message)));
                    return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
                }

                // Complete Transaction
                transaction.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred during blob creation transaction for '{request.FileBaseName}.{request.FileExtension}'. Database changes rolled back.");
                return Result.Fail<BlobStorageFile>(ErrorMessages.GenericErrorWithRetryPrompt);
            }
        }
        
        if (addResult.IsFailed) {
             _logger.LogError(new Exception("Reached end of CreateAndUploadBlobCommandHandler successfully, but addResult was null or failed unexpectedly."), "Unexpected state.");
             return Result.Fail<BlobStorageFile>(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        _logger.LogInformation($"Successfully created blob metadata ID {addResult.Value.Id} and uploaded blob '{blobName}'.");
        return Result.Ok(addResult.Value);
    }
}
