using AutoFixture;
using AutoFixture.Kernel;
using DomainModules.BlobStorage.Entities;

namespace ServiceModules.Tests.Config.Entities.BlobStorage;

public class BlobStorageFileSpecimenBuilder : IEntitySpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(BlobStorageFile))
        {
            string fileBaseName = context.Create<string>();
            string fileExtension = "pdf";
            string contentType = "application/pdf";
            byte[] dummyBytes = new byte[1024];
            var memoryStream = new MemoryStream(dummyBytes);

            return new BlobStorageFile
            {
                FileBaseName = fileBaseName,
                FileExtension = fileExtension,
                ContentType = contentType,
                FileContent = memoryStream
            };
        }

        return new NoSpecimen();
    }
}
