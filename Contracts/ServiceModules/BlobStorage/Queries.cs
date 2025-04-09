using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.BlobStorage;

public record GetBlobMetadataQuery(int Id) : IRequest<Result<BlobStorageFile>>;
public record GetBulkBlobMetadataQuery(List<int> Ids) : IRequest<Result<List<BlobStorageFile>>>;
