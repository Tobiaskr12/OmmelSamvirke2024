using Contracts.Infrastructure.BlobStorage;
using Contracts.ServiceModules.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.BlobStorage.Commands;

public class DownloadBlobCommandHandler : IRequestHandler<DownloadBlobCommand, Result<BlobStorageFile>>
{
    private readonly IBlobStorageService _blobStorageService;
        
    public DownloadBlobCommandHandler(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }
        
    public async Task<Result<BlobStorageFile>> Handle(DownloadBlobCommand request, CancellationToken cancellationToken)
    {
        BlobStorageFile blob = request.BlobStorageFile;
            
        Result<BlobStorageFile> downloadResult = await _blobStorageService.DownloadBlobAsync(blob, cancellationToken);
        return downloadResult;
    }
}
