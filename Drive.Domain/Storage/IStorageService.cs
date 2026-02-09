namespace Drive.Domain.Storage;

public interface IStorageService
{
    Task EnsureBucketAsync(CancellationToken ct);

    Task UploadAsync(
        Stream content,
        string objectKey,
        string contentType,
        CancellationToken ct);

    Task<Stream> DownloadAsync(
        string objectKey,
        CancellationToken ct);

    Task DeleteAsync(
        string objectKey,
        CancellationToken ct);
}
