using ImageProcessor.Abstractions;
using ImageProcessor.Domain;
using ImageProcessor.Infrastructure;
using Moq;
using Xunit;

namespace ImageProcessor.Tests;

public class RepositoryTests
{
    [Fact]
    public void Add_StoresTask_ThenGetById_ReturnsIt()
    {
        // Arrange
        MemoryImageRepository repository = new MemoryImageRepository();
        ImageTask task = new ImageTask { OriginalFileName = "a.jpg" };

        // Act
        repository.Add(task);
        var fetched = repository.GetById(task.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(task.Id, fetched!.Id);
        Assert.Equal("a.jpg", fetched!.OriginalFileName);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        MemoryImageRepository repository = new MemoryImageRepository();

        Assert.Null(repository.GetById(Guid.NewGuid()));
    }

    [Fact]
    public void UpdateStatus_SetsStatus_AndProcessedFilePath()
    {
        // Arrange
        MemoryImageRepository repository = new MemoryImageRepository();
        ImageTask task = new ImageTask { OriginalFileName = "a.jpg" };
        repository.Add(task);

        // Act
        repository.UpdateStatus(task.Id, ImageStatus.Processing);
        repository.UpdateStatus(task.Id, ImageStatus.Completed, "/processed/out.jpg");

        // Assert
        var updated = repository.GetById(task.Id);
        Assert.NotNull(updated);
        Assert.Equal(ImageStatus.Completed, updated!.Status);
        Assert.Equal("/processed/out.jpg", updated!.ProcessedFilePath);
    }

    [Fact]
    public void UpdateStatus_UnknownId_DoesNotThrow()
    {
        MemoryImageRepository repository = new MemoryImageRepository();

        repository.UpdateStatus(Guid.NewGuid(), ImageStatus.Failed);
    }
}
