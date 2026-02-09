using Drive.Domain.Repositories;
using Drive.Domain.Storage;

namespace Drive.Business.UseCases.DeleteFile;

public class DeleteFileUseCase
{
    private readonly IFileRepository _files;
    private readonly IStorageService _storage;

    public DeleteFileUseCase(IFileRepository files, IStorageService storage)
    {
        _files = files;
        _storage = storage;
    }

    public async Task ExecuteAsync(DeleteFileRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.OwnerUserId))
            throw new ArgumentException("OwnerUserId é obrigatório.");

        var file = await _files.GetByIdAsync(req.FileId, ct);
        if (file is null)
            throw new KeyNotFoundException("Arquivo não encontrado.");

        // segurança: garante que o arquivo é do usuário
        if (file.OwnerUserId != req.OwnerUserId)
            throw new UnauthorizedAccessException("Você não tem acesso a este arquivo.");

        // 1) apaga do storage (MinIO)
        await _storage.DeleteAsync(file.StoredKey, ct);

        // 2) apaga do banco
        await _files.DeleteAsync(file, ct);
    }
}
