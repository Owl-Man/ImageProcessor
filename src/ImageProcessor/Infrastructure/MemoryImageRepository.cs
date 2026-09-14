using System.Collections.Concurrent;
using ImageProcessor.Abstractions;
using ImageProcessor.Domain;

namespace ImageProcessor.Infrastructure;

public class MemoryImageRepository : IImageRepository 
{
    private readonly ConcurrentDictionary<Guid, ImageTask> _tasks = new();

    public void Add(ImageTask task) => _tasks[task.Id] = task;

    public ImageTask? GetById(Guid id) => _tasks.TryGetValue(id, out ImageTask? task) ? task : null;


    public void UpdateStatus(Guid id, ImageStatus status, string? processedFilePath = null) 
    {
        if (_tasks.TryGetValue(id, out ImageTask? task)) 
        {
            task.Status = status;

            if (processedFilePath != null) 
            {
                task.ProcessedFilePath = processedFilePath;
            }
        }
    }
}
