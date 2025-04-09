using Contracts.DataAccess.Base;
using Contracts.ServiceModules.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.BlobStorage.Queries;

public class GetBulkBlobMetadataQueryHandler : IRequestHandler<GetBulkBlobMetadataQuery, Result<List<BlobStorageFile>>>
{
    private readonly IRepository<BlobStorageFile> _repository;

    public GetBulkBlobMetadataQueryHandler(IRepository<BlobStorageFile> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<BlobStorageFile>>> Handle(GetBulkBlobMetadataQuery request, CancellationToken cancellationToken)
    {
        Result<List<BlobStorageFile>> result = await _repository.FindAsync(x => request.Ids.Contains(x.Id), true, cancellationToken);
        if (result.IsFailed || result.Value == null || result.Value.Count == 0)
        {
            return Result.Fail<List<BlobStorageFile>>(ErrorMessages.GenericNotFound);
        }
        return Result.Ok(result.Value);
    }
}