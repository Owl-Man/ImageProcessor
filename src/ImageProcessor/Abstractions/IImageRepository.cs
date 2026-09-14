using ImageProcessor.Domain;

namespace ImageProcessor.Abstractions;

public interface IImageRepository
{
    void Add(ImageTask task);
    ImageTask? GetById(Guid id);
    void UpdateStatus(Guid id, ImageStatus status, string? processedFile = null);
}
