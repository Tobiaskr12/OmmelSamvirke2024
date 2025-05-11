using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.BlobStorage;

public record CreateAndUploadBlobCommand(
    Stream FileContent,
    string FileBaseName,
    string FileExtension,
    string ContentType
) : IRequest<Result<BlobStorageFile>>; 

public record DeleteBlobCommand(int BlobMetadataId) : IRequest<Result>;