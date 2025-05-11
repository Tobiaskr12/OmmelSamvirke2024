using Contracts.DataAccess.Base;
using Contracts.ServiceModules.AlbumImages;
using Contracts.SupportModules.Logging;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.ImageAlbums.Commands;

public class SetAlbumCoverImageCommandHandler : IRequestHandler<SetAlbumCoverImageCommand, Result>
{
    private readonly IRepository<Album> _albumRepository;
    private readonly IRepository<Image> _imageRepository;
    private readonly ILoggingHandler _logger;

    public SetAlbumCoverImageCommandHandler(
        IRepository<Album> albumRepository,
        IRepository<Image> imageRepository,
        ILoggingHandler logger)
    {
        _albumRepository = albumRepository;
        _imageRepository = imageRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(SetAlbumCoverImageCommand request, CancellationToken cancellationToken)
    {
        Result<Album> findAlbumResult = await _albumRepository.GetByIdAsync(
            request.AlbumId,
            readOnly: false,
            cancellationToken: cancellationToken
        );
        
        if (findAlbumResult.IsFailed)
        {
             _logger.LogError(new Exception($"Failed to retrieve album with ID {request.AlbumId} for setting cover image."), string.Join(", ", findAlbumResult.Errors.Select(e => e.Message)));
             return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        Album foundAlbum = findAlbumResult.Value;

        // Verify image exists
        Result<Image> imageResult = await _imageRepository.GetByIdAsync(
            request.ImageId,
            readOnly: false,
            cancellationToken: cancellationToken
        );
        if (imageResult.IsFailed)
        {
             _logger.LogError(
                 new Exception($"Failed to retrieve image with ID {request.ImageId} for setting as cover."),
                 string.Join(", ", imageResult.Errors.Select(e => e.Message))
             );
             return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        if (imageResult.Value is null)
        {
            _logger.LogInformation($"Attempted to set non-existent image (ID: {request.ImageId}) as cover for Album ID: {request.AlbumId}");
            return Result.Fail(ErrorMessages.GenericNotFound);
        }

        // Set and save cover image
        foundAlbum.CoverImage = imageResult.Value;
        Result<Album> updateResult = await _albumRepository.UpdateAsync(foundAlbum, cancellationToken);
        if (updateResult.IsFailed)
        {
             _logger.LogError(
                 new Exception($"Failed to update Album ID: {request.AlbumId} while setting cover image ID: {request.ImageId}"),
                 string.Join(", ", updateResult.Errors.Select(e => e.Message))
             );
             return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        _logger.LogInformation($"Successfully set Image ID: {request.ImageId} as cover for Album ID: {request.AlbumId}");
        return Result.Ok();
    }
}
