using Drive.Domain.Repositories;

namespace Drive.Business.UseCases.ListFiles;

public class ListFilesUseCase
{
    private readonly IFileRepository _files;

    public ListFilesUseCase(IFileRepository files)
    {
        _files = files;
    }

    public async Task<ListFilesResponse> ExecuteAsync(ListFilesRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.OwnerUserId))
            throw new ArgumentException("OwnerUserId é obrigatório.");

        var items = await _files.ListAsync(req.OwnerUserId, req.Search, ct);

        var mapped = items
            .Select(f => new ListFilesItemResponse(
                Id: f.Id,
                OriginalName: f.OriginalName,
                SizeInBytes: f.SizeInBytes,
                ContentType: f.ContentType,
                CreatedAtUtc: f.CreatedAtUtc
            ))
            .ToList();

        return new ListFilesResponse(mapped);
    }
}
