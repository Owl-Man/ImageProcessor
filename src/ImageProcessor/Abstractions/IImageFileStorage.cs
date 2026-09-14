using ImageProcessor.Domain;

namespace ImageProcessor.Abstractions;

public interface IImageFileStorage 
{
    Task<StoredImage> SaveOriginalAsync(
            IFormFile file,
            CancellationToken cancellationToken = default);
}
