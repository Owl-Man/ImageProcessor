using ImageProcessor.Domain;

namespace ImageProcessor.Abstractions;

public interface ITaskPublisher
{
   Task PublishProcessImageAsync(ProcessImageMessage message,
           CancellationToken cancellationToken = default);
}
