using ImageProcessor.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessor.Infrastructure;


public class ImageSharpResizer : IImageResizer
{
    private readonly string _rootPath;

    public ImageSharpResizer(string rootPath)
    {
       _rootPath = rootPath; 
    }

    public async Task<string> ResizeToMaxAsync(string fileName, int maxWidth, int maxHeight, 
            CancellationToken cancellationToken = default) 
    {
        string sourcePath = Path.Combine(_rootPath, "originals", fileName);

        string resultFileName = $"processed_{Path.GetFileNameWithoutExtension(fileName)}.jpg";
        string resultPath = Path.Combine(_rootPath, "processed", resultFileName);


        Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);

        using Image image = await Image.LoadAsync(sourcePath, cancellationToken);

        image.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(maxWidth, maxHeight)
        }));

        await image.SaveAsJpegAsync(resultPath, cancellationToken);

        return resultFileName;
    }
}
