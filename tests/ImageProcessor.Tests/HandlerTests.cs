using ImageProcessor.Abstractions;
using ImageProcessor.Domain;
using ImageProcessor.Services;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ImageProcessor.Tests;

public class HandlerTests
{
    private static (ImageProcessingHandler handler, Mock<IImageRepository> repository, Mock<IImageResizer> resizer)
        CreateHandler()
    {
        Mock<IImageRepository> repository = new Mock<IImageRepository>();
        Mock<IImageResizer> resizer = new Mock<IImageResizer>();
        Mock<ILogger<ImageProcessingHandler>> logger = new Mock<ILogger<ImageProcessingHandler>>();

        ImageProcessingHandler handler = new ImageProcessingHandler(repository.Object, resizer.Object, logger.Object);
        return (handler, repository, resizer);
    }

    [Fact]
    public async Task HandleAsync_Success_MarksTaskCompletedWithProcessedPath()
    {
        // Arrange
        var (handler, repository, resizer) = CreateHandler();
        ImageTask task = new ImageTask();
        repository.Setup(r => r.GetById(task.Id)).Returns(task);

        resizer.Setup(r => r.ResizeToMaxAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("processed_orig.jpg");

        ProcessImageMessage message = new ProcessImageMessage(task.Id, "orig.jpg");

        // Act
        await handler.HandleAsync(message);

        // Assert
        repository.Verify(
            r => r.UpdateStatus(task.Id, ImageStatus.Completed, "processed_orig.jpg"),
            Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ResizerThrows_MarksTaskFailed()
    {
        // Arrange
        var (handler, repository, resizer) = CreateHandler();
        ImageTask task = new ImageTask();
        repository.Setup(r => r.GetById(task.Id)).Returns(task);

        resizer.Setup(r => r.ResizeToMaxAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        ProcessImageMessage message = new ProcessImageMessage(task.Id, "orig.jpg");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(message));

        repository.Verify(
            r => r.UpdateStatus(task.Id, ImageStatus.Failed, It.IsAny<string?>()),
            Times.Once());
    }
}
