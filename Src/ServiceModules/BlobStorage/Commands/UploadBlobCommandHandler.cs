using Contracts.DataAccess.Base;
using Contracts.Infrastructure.BlobStorage;
using Contracts.ServiceModules.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.BlobStorage.Commands;

public class UploadBlobCommandHandler : IRequestHandler<UploadBlobCommand, Result<BlobStorageFile>>
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IRepository<BlobStorageFile> _repository;
        
    public UploadBlobCommandHandler(IBlobStorageService blobStorageService, IRepository<BlobStorageFile> repository)
    {
        _blobStorageService = blobStorageService;
        _repository = repository;
    }
        
    public async Task<Result<BlobStorageFile>> Handle(UploadBlobCommand request, CancellationToken cancellationToken)
    {
        BlobStorageFile blob = request.BlobStorageFile;
            
        Result uploadResult = await _blobStorageService.UploadBlobAsync(blob, request.FileContent, cancellationToken);
        if (uploadResult.IsFailed)
        {
            return Result.Fail<BlobStorageFile>(uploadResult.Errors);
        }
        
        Result<BlobStorageFile> addResult = await _repository.AddAsync(blob, cancellationToken);
        return addResult.IsFailed 
            ? Result.Fail<BlobStorageFile>(addResult.Errors) 
            : Result.Ok(blob);
    }
}
