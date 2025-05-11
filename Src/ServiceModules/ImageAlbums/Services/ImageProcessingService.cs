using Contracts.ServiceModules.AlbumImages;
using FluentResults;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ServiceModules.ImageAlbums.Services;

public sealed class ImageProcessingService : IImageProcessingService
{
    private const int DefaultMaxSide = 1600;
    private const int DefaultMinSide = 800;
    private const int ThumbnailMaxSide = 300;

    // high-quality, 4:4:4 subsampling, progressive
    private static readonly JpegEncoder DefaultJpegEncoder = new()
    {
        Quality = 90,
        Interleaved = true,
        ColorType = JpegEncodingColor.YCbCrRatio444,
        SkipMetadata = true
    };

    // slightly lower quality for thumbnails
    private static readonly JpegEncoder ThumbJpegEncoder = new()
    {
        Quality = 80,
        Interleaved = true,
        ColorType = JpegEncodingColor.YCbCrRatio444,
        SkipMetadata = true
    };

    public async Task<Result<ProcessedImageVersions>> GenerateVersionsAsync(
        byte[] originalImageContent,
        string originalContentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(
                new MemoryStream(originalImageContent), 
                CancellationToken.None
            );

            // Default version
            (int defW, int defH) = CalcTargetSize(
                image.Width, image.Height,
                DefaultMaxSide, DefaultMinSide
            );

            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(defW, defH),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3,
                Compand = true,
                PremultiplyAlpha = true
            }));

            byte[] defaultJpeg;
            await using (var ms = new MemoryStream())
            {
                await image.SaveAsJpegAsync(ms, DefaultJpegEncoder, CancellationToken.None);
                defaultJpeg = ms.ToArray();
            }

            // Thumbnail version
            using Image<Rgba32> thumbImage = await Image.LoadAsync<Rgba32>(
                new MemoryStream(originalImageContent),
                CancellationToken.None
            );
            (int thumbW, int thumbH) = CalcTargetSize(
                thumbImage.Width, thumbImage.Height,
                ThumbnailMaxSide, null
            );
            thumbImage.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(thumbW, thumbH),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3,
                Compand = true,
                PremultiplyAlpha = true
            }));

            byte[] thumbJpeg;
            await using (var ms = new MemoryStream())
            {
                await thumbImage.SaveAsJpegAsync(ms, ThumbJpegEncoder, CancellationToken.None);
                thumbJpeg = ms.ToArray();
            }

            return Result.Ok(new ProcessedImageVersions(
                DefaultVersionContent: defaultJpeg,
                DefaultVersionContentType: "image/jpeg",
                ThumbnailVersionContent: thumbJpeg,
                ThumbnailVersionContentType: "image/jpeg"
            ));
        }
        catch (Exception ex)
        {
            return Result.Fail<ProcessedImageVersions>(ex.Message);
        }
    }

    private static (int w, int h) CalcTargetSize(int srcWidth, int srcHeight, int maxSide, int? minSide)
    {
        int width = srcWidth, height = srcHeight;
        int longSide = Math.Max(width, height);

        // Upscale if below minSide
        if (longSide < minSide)
        {
            float f = (float)minSide.Value / longSide;
            width = (int)Math.Round(width * f);
            height = (int)Math.Round(height * f);
            longSide = minSide.Value;
        }

        // Downscale if above maxSide
        if (longSide > maxSide)
        {
            float f = (float)maxSide / longSide;
            width = (int)Math.Round(width * f);
            height = (int)Math.Round(height * f);
        }

        return (width, height);
    }
}
