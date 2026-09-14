namespace ImageProcessor.Abstractions;

public interface IImageResizer
{
   Task<string> ResizeToMaxAsync(string fileName, 
           int maxWidth,
           int maxHeight,
           CancellationToken cancellationToken = default);
}
