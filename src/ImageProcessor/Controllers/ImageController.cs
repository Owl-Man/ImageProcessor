using ImageProcessor.Abstractions;
using ImageProcessor.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IImageRepository _repository;
    private readonly ITaskPublisher _publisher;
    private readonly IImageFileStorage _fileStorage;

    public ImageController(IImageRepository repository, ITaskPublisher publisher, IImageFileStorage fileStorage)
    {
       _repository = repository;
       _publisher = publisher;
       _fileStorage = fileStorage;
    }

    [HttpPost("process")]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> Process([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new {error = "file is required"});

        StoredImage stored = await _fileStorage.SaveOriginalAsync(file, HttpContext.RequestAborted);

        ImageTask task = new ImageTask { OriginalFileName = stored.FileName };

        _repository.Add(task);

        await _publisher.PublishProcessImageAsync(new ProcessImageMessage(task.Id, stored.FileName), HttpContext.RequestAborted);

        return AcceptedAtAction(nameof(GetStatus), new {id = task.Id}, new {taskId = task.Id, status = task.Status});
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetStatus(Guid id)
    {
        ImageTask? task = _repository.GetById(id);

        if (task is null)
            return NotFound();

        return Ok(task);
    }
}
