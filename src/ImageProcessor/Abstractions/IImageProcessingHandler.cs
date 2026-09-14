using ImageProcessor.Domain;

namespace ImageProcessor.Abstractions;

public interface IImageProcessingHandler
{
   Task HandleAsync(ProcessImageMessage message, CancellationToken cancellationToken = default);
}
