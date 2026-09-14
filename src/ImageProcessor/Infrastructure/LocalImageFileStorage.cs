using ImageProcessor.Abstractions;
using ImageProcessor.Domain;

namespace ImageProcessor.Infrastructure;

public class LocalImageFileStorage(string rootPath) : IImageFileStorage 
{
    private readonly string _rootPath = rootPath;

    public async Task<StoredImage> SaveOriginalAsync(IFormFile file, CancellationToken cancellationToken = default) 
    {
        string saveFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        string directory = Path.Combine(_rootPath, "originals");
        Directory.CreateDirectory(directory);

        string fullPath = Path.Combine(directory, "originals");

        await using FileStream target = File.Create(fullPath);
        await file.CopyToAsync(target, cancellationToken);

        return new StoredImage(saveFileName, fullPath);
    }
}
