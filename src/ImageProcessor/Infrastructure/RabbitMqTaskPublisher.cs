using System.Text;
using System.Text.Json;
using ImageProcessor.Abstractions;
using ImageProcessor.Domain;
using RabbitMQ.Client;

namespace ImageProcessor.Infrastructure;

public class RabbitMqTaskPublisher(IConnection connection) : ITaskPublisher
{
    private const string QueueName = "image_processing_queue";

    private readonly IConnection _connection = connection;

    public async Task PublishProcessImageAsync(ProcessImageMessage message, CancellationToken cancellationToken = default)
    {
        using IChannel channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete:false,
            cancellationToken: cancellationToken);

        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = new BasicProperties { Persistent = true};

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueName,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
