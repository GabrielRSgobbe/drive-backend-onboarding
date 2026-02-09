using Drive.Domain.Entities;
using Drive.Domain.Repositories;
using Drive.Domain.Storage;

namespace Drive.Business.UseCases.UploadFile;

public class UploadFileUseCase
{
    private readonly IFileRepository _files;
    private readonly IStorageService _storage;

    public UploadFileUseCase(IFileRepository files, IStorageService storage)
    {
        _files = files;
        _storage = storage;
    }

    public async Task<UploadFileResponse> ExecuteAsync(UploadFileRequest req, CancellationToken ct)
    {
        // Regras/validações mínimas (MVP)
        if (string.IsNullOrWhiteSpace(req.OwnerUserId))
            throw new ArgumentException("OwnerUserId é obrigatório.");

        if (req.Content is null)
            throw new ArgumentException("Conteúdo do arquivo é obrigatório.");

        if (req.SizeInBytes <= 0)
            throw new ArgumentException("Tamanho do arquivo inválido.");

        if (string.IsNullOrWhiteSpace(req.OriginalName))
            throw new ArgumentException("Nome do arquivo é obrigatório.");

        // Chave única no storage (evita colisão)
        var safeName = req.OriginalName.Replace(" ", "_");
        var storedKey = $"{Guid.NewGuid():N}-{safeName}";

        // 1) subir para o storage
        await _storage.UploadAsync(req.Content, storedKey, req.ContentType, ct);

        // 2) salvar metadados no banco
        var file = new FileItem(
            originalName: req.OriginalName,
            storedKey: storedKey,
            contentType: req.ContentType,
            sizeInBytes: req.SizeInBytes,
            ownerUserId: req.OwnerUserId
        );

        await _files.AddAsync(file, ct);

        // 3) resposta
        return new UploadFileResponse(
            Id: file.Id,
            OriginalName: file.OriginalName,
            SizeInBytes: file.SizeInBytes,
            ContentType: file.ContentType,
            CreatedAtUtc: file.CreatedAtUtc
        );
    }
}
