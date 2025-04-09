using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.BlobStorage;

public record UploadBlobCommand(Stream FileContent, BlobStorageFile BlobStorageFile) : IRequest<Result<BlobStorageFile>>;
public record DownloadBlobCommand(BlobStorageFile BlobStorageFile) : IRequest<Result<BlobStorageFile>>;
