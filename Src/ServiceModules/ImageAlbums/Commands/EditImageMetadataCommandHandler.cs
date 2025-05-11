using Contracts.DataAccess.Base;
using Contracts.ServiceModules.AlbumImages;
using Contracts.SupportModules.Logging;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.ImageAlbums.Commands;

public class EditImageMetadataCommandHandler : IRequestHandler<EditImageMetadataCommand, Result<Image>>
{
    private readonly IRepository<Image> _imageRepository;
    private readonly ILoggingHandler _logger;

    public EditImageMetadataCommandHandler(IRepository<Image> imageRepository, ILoggingHandler logger)
    {
        _imageRepository = imageRepository;
        _logger = logger;
    }

    public async Task<Result<Image>> Handle(EditImageMetadataCommand request, CancellationToken cancellationToken)
    {
        Result<Image> imageResult = await _imageRepository.GetByIdAsync(request.ImageId, readOnly: false, cancellationToken);

        if (imageResult.IsFailed || imageResult.Value is null)
        {
            _logger.LogInformation($"Attempted to edit metadata for non-existent image with ID: {request.ImageId}");
            return Result.Fail(ErrorMessages.GenericNotFound);
        }

        Image imageToUpdate = imageResult.Value;
        
        imageToUpdate.DateTaken = request.DateTaken;
        imageToUpdate.Location = request.Location;
        imageToUpdate.PhotographerName = request.PhotographerName;
        imageToUpdate.Title = request.Title;
        imageToUpdate.Description = request.Description;

        // Update image meta-data changes
        Result<Image> updateResult = await _imageRepository.UpdateAsync(imageToUpdate, cancellationToken);

        if (updateResult.IsFailed)
        {
            _logger.LogError(
                new Exception($"Failed to update metadata for Image ID: {request.ImageId}"),
                string.Join(", ", updateResult.Errors.Select(e => e.Message))
            );
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }

        _logger.LogInformation($"Successfully updated metadata for Image ID: {request.ImageId}");
        return Result.Ok(updateResult.Value);
    }
}
