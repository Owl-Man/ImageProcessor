using ImageProcessor.Abstractions;
using ImageProcessor.Infrastructure;
using ImageProcessor.Services;
using ImageProcessor.Workers;
using Microsoft.Extensions.FileProviders;
using RabbitMQ.Client;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string storageRoot = Path.Combine(builder.Environment.ContentRootPath, "storage");

Directory.CreateDirectory(Path.Combine(storageRoot, "originals"));
Directory.CreateDirectory(Path.Combine(storageRoot, "processed"));

ConnectionFactory rabbitFactory = new ConnectionFactory 
{
    HostName = builder.Configuration["RabbitMQ:Host"],
    Port = int.TryParse(builder.Configuration["RabbitMQ:Port"], out int port) ? port : 5672,
    UserName = builder.Configuration["RabbitMQ:UserName"],
    Password = builder.Configuration["RabbitMQ:Password"],
};

IConnection rabbitConnection = await rabbitFactory.CreateConnectionAsync();


builder.Services.AddSingleton<IConnection>(rabbitConnection);
builder.Services.AddSingleton<IImageRepository, MemoryImageRepository>();
builder.Services.AddSingleton<IImageFileStorage>(new LocalImageFileStorage(storageRoot));
builder.Services.AddSingleton<IImageResizer>(new ImageSharpResizer(storageRoot));
builder.Services.AddSingleton<ITaskPublisher, RabbitMqTaskPublisher>();
builder.Services.AddSingleton<IImageProcessingHandler, ImageProcessingHandler>();

builder.Services.AddHostedService<ImageProcessingWorker>();

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(storageRoot, "processed")),
    RequestPath = "/processed-images"
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

