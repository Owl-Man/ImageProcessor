namespace ImageProcessor.Domain;

public enum ImageStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
}

public class ImageTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalFileName {get;set;} = string.Empty;
    public ImageStatus Status { get; set; } = ImageStatus.Pending;
    public string? ProcessedFilePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public record ProcessImageMessage(Guid TaskId, string FileName);
public record StoredImage(string FileName, string FullPath);
