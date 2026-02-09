using Drive.Domain.Storage;
using Minio;
using Minio.DataModel.Args;

namespace Drive.Infrastructure.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minio;
    private readonly string _bucket;

    public MinioStorageService(IMinioClient minio, string bucket)
    {
        _minio = minio;
        _bucket = bucket;
    }

    public async Task EnsureBucketAsync(CancellationToken ct)
    {
        var exists = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket), ct);
        if (!exists)
            await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket), ct);
    }

    public async Task UploadAsync(Stream content, string objectKey, string contentType, CancellationToken ct)
    {
        await EnsureBucketAsync(ct);

        // garante posição no início, importante se o stream já foi lido
        if (content.CanSeek) content.Position = 0;

        await _minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType), ct);
    }

    public async Task<Stream> DownloadAsync(string objectKey, CancellationToken ct)
    {
        await EnsureBucketAsync(ct);

        var ms = new MemoryStream();

        await _minio.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(ms)), ct);

        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(string objectKey, CancellationToken ct)
    {
        await EnsureBucketAsync(ct);

        await _minio.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey), ct);
    }
}
