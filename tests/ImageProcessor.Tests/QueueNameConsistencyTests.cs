using ImageProcessor.Infrastructure;
using ImageProcessor.Workers;
using System.Reflection;
using Xunit;

namespace ImageProcessor.Tests;

public class QueueNameConsistencyTests
{
    [Fact]
    public void PublisherAndWorker_DeclareSameQueueName()
    {
        // The publisher and the worker must declare the exact same queue name,
        // otherwise RabbitMQ treats them as two different queues (names are case-sensitive).

        string? GetConstValue(Type type, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            return field?.GetValue(null) as string;
        }

        string? publisherQueue = GetConstValue(typeof(RabbitMqTaskPublisher), "QueueName");
        string? workerQueue = GetConstValue(typeof(ImageProcessingWorker), "QueueName");

        Assert.NotNull(publisherQueue);
        Assert.NotNull(workerQueue);
        Assert.Equal(publisherQueue, workerQueue);
    }
}
