using ImageProcessor.Abstractions;
using ImageProcessor.Domain;

namespace ImageProcessor.Services;

public class ImageProcessingHandler : IImageProcessingHandler 
{
    private readonly IImageRepository _repository;
    private readonly IImageResizer _resizer;
    private readonly ILogger<ImageProcessingHandler> _logger;

    public ImageProcessingHandler(IImageRepository repository, IImageResizer resizer, ILogger<ImageProcessingHandler> logger)
    {
        _repository = repository;
        _resizer = resizer;
        _logger = logger;
    }

    public async Task HandleAsync(ProcessImageMessage message,CancellationToken cancellationToken = default)
    {
        _repository.UpdateStatus(message.TaskId, ImageStatus.Processing);

        try
        {
            string processedFileName = await _resizer.ResizeToMaxAsync(
                message.FileName, maxWidth: 800, maxHeight: 800, cancellationToken);

            _repository.UpdateStatus(message.TaskId, ImageStatus.Completed, processedFileName);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) 
        {
            _logger.LogError(exception, "Error with image processing {TaskId}", message.TaskId);

            _repository.UpdateStatus(message.TaskId, ImageStatus.Failed);

            throw;
        }
    }
}
