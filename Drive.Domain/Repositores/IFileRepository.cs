using Drive.Domain.Entities;

namespace Drive.Domain.Repositories;

public interface IFileRepository
{
    Task AddAsync(FileItem file, CancellationToken ct);
    Task<FileItem?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<FileItem>> ListAsync(string ownerUserId, string? search, CancellationToken ct);
    Task DeleteAsync(FileItem file, CancellationToken ct);
}
