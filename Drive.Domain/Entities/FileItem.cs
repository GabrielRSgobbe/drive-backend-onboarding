namespace Drive.Domain.Entities;

public class FileItem
{
    public Guid Id { get; private set; }
    public string OriginalName { get; private set; }
    public string StoredKey { get; private set; }
    public string ContentType { get; private set; }
    public long SizeInBytes { get; private set; }
    public string OwnerUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // Construtor para criar um novo arquivo
    public FileItem(
        string originalName,
        string storedKey,
        string contentType,
        long sizeInBytes,
        string ownerUserId)
    {
        Id = Guid.NewGuid();
        OriginalName = originalName;
        StoredKey = storedKey;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        OwnerUserId = ownerUserId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    // Construtor vazio (necessário para EF Core no futuro)
    protected FileItem() { }
}
