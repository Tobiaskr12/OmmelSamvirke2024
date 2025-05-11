using DataAccess.Base;
using DomainModules.BlobStorage.Entities;
using DomainModules.ImageAlbums.Entities;

namespace ServiceModules.Tests.ImageAlbums;

public class TestDataFactory : ServiceTestBase
{
    /// <summary>
    /// Persists three BlobStorageFile entities, one Album, and one Image linking them.
    /// </summary>
    public static async Task<Image> CreateAndPersistImageWithAlbumAsync()
    {
        // Persist blobs
        var orig  = new BlobStorageFile { FileBaseName = "i_org",  FileExtension = "jpg", ContentType = "image/jpeg" };
        var def   = new BlobStorageFile { FileBaseName = "i_def",  FileExtension = "jpg", ContentType = "image/jpeg" };
        var thumb = new BlobStorageFile { FileBaseName = "i_thumb",FileExtension = "jpg", ContentType = "image/jpeg" };
        orig.SetFileSize(3); def.SetFileSize(3); thumb.SetFileSize(3);

        await AddTestData(orig);
        await AddTestData(def);
        await AddTestData(thumb);

        // Persist album
        var album = new Album { Name = "ImgAlbum", Images = new List<Image>(), CoverImage = null };
        await AddTestData(album);

        // Persist image
        var image = new Image
        {
            OriginalBlobStorageFile  = orig,
            DefaultBlobStorageFile   = def,
            ThumbnailBlobStorageFile = thumb,
            Album = album
        };
        await AddTestData(image);

        // Clear the change tracker so none of the created entities are already tracked when i fetch them in the tests
        var db = GetService<OmmelSamvirkeDbContext>();
        db.ChangeTracker.Clear();

        return image;
    }
    
    public static async Task<Image> CreateAndPersistImageWithAlbumAsync(Album album)
    {
        // Persist blobs
        var orig  = new BlobStorageFile { FileBaseName = "i_org",  FileExtension = "jpg", ContentType = "image/jpeg" };
        var def   = new BlobStorageFile { FileBaseName = "i_def",  FileExtension = "jpg", ContentType = "image/jpeg" };
        var thumb = new BlobStorageFile { FileBaseName = "i_thumb",FileExtension = "jpg", ContentType = "image/jpeg" };
        orig.SetFileSize(3); def.SetFileSize(3); thumb.SetFileSize(3);
        await AddTestData(orig);
        await AddTestData(def);
        await AddTestData(thumb);

        // Persist image
        var image = new Image
        {
            OriginalBlobStorageFile  = orig,
            DefaultBlobStorageFile   = def,
            ThumbnailBlobStorageFile = thumb,
            Album                    = album,
            Title                    = $"Title{Guid.NewGuid()}",
            Description              = $"Desc{Guid.NewGuid()}"
        };
        await AddTestData(image);
        return image;
    }

    /// <summary>
    /// Returns the three blob‐names for uploading/deleting in storage.
    /// </summary>
    public static string[] GetBlobNames(Image img) =>
    [
        $"{img.OriginalBlobStorageFile.Id}.{img.OriginalBlobStorageFile.FileExtension}",
        $"{img.DefaultBlobStorageFile.Id}.{img.DefaultBlobStorageFile.FileExtension}",
        $"{img.ThumbnailBlobStorageFile.Id}.{img.ThumbnailBlobStorageFile.FileExtension}"
    ];

    /// <summary>
    /// Returns the three full blobs
    /// </summary>
    public static BlobStorageFile[] GetBlobStorageFiles(Image img) =>
    [
        img.OriginalBlobStorageFile,
        img.DefaultBlobStorageFile,
        img.ThumbnailBlobStorageFile
    ];
}
