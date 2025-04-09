using Contracts.DataAccess.Base;
using Contracts.ServiceModules.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.BlobStorage.Queries;

public class GetBlobMetadataQueryHandler : IRequestHandler<GetBlobMetadataQuery, Result<BlobStorageFile>>
{
    private readonly IRepository<BlobStorageFile> _repository;

    public GetBlobMetadataQueryHandler(IRepository<BlobStorageFile> repository)
    {
        _repository = repository;
    }

    public async Task<Result<BlobStorageFile>> Handle(GetBlobMetadataQuery request, CancellationToken cancellationToken)
    {
        Result<BlobStorageFile> result = await _repository.GetByIdAsync(request.Id, true, cancellationToken);
        if (result.IsFailed || result.Value == null)
        {
            return Result.Fail<BlobStorageFile>(ErrorMessages.GenericNotFound);
        }
            
        return Result.Ok(result.Value);
    }
}
