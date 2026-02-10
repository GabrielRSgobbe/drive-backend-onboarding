using Drive.Domain.Repositories;
using Drive.Domain.Storage;

namespace Drive.Business.UseCases.DownloadFile;

public class DownloadFileUseCase
{
    private readonly IFileRepository _files;
    private readonly IStorageService _storage;

    public DownloadFileUseCase (IFileRepository files , IStorageService storage)
    {
        _files = files;
        _storage = storage;
    }

    public async Task<DownloadFileResponse> ExecuteAsync(DownloadFileRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.OwnerUserId))
        throw new ArgumentException("OwnerUserId é obrigatorio");

        var file = await _files.GetByIdAsync(req.FileId, ct);
        if (file is null)
            throw new KeyNotFoundException("Arquivo não encontrado");

        if(file.OwnerUserId != req.OwnerUserId)
            throw new UnauthorizedAccessException("Você não tem acesso a esse arquivo");

        var content = await _storage.DownloadAsync(file.StoredKey,ct);


        return new DownloadFileResponse(
            Content: content,
            ContentType: string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream": file.ContentType,
            FileName: file.OriginalName
        );
        

    }

}