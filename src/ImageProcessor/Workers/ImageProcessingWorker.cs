using System.Text;
using System.Text.Json;
using ImageProcessor.Abstractions;
using ImageProcessor.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ImageProcessor.Workers;

public class ImageProcessingWorker : BackgroundService 
{
    private const string QueueName = "image_processing_queue";

    private readonly IImageProcessingHandler _handler;
    private readonly ILogger<ImageProcessingWorker> _logger;
    private readonly IConnection _connection;

    public ImageProcessingWorker(IImageProcessingHandler handler, ILogger<ImageProcessingWorker> logger, IConnection connection)
    {
       _handler = handler;
       _logger = logger;
       _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using IChannel channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(queue: QueueName,
                durable:true,
                exclusive:false,
                autoDelete:false,
                cancellationToken: cancellationToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) => 
        {
            try
            {
                byte[] body = ea.Body.ToArray();
                string json = Encoding.UTF8.GetString(body);

                ProcessImageMessage? message = JsonSerializer.Deserialize<ProcessImageMessage>(json);

                if (message != null) await _handler.HandleAsync(message, cancellationToken);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (OperationCanceledException)
            {
               await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue:true);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error with task processing in queue");

               await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue:false);
            }
        };

        await channel.BasicConsumeAsync(queue: QueueName, autoAck:false, consumer:consumer, cancellationToken: cancellationToken);

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException) {}
    }
}

