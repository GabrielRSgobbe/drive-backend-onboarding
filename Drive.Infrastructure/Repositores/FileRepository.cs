using Drive.Domain.Entities;
using Drive.Domain.Repositories;
using Drive.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Drive.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly AppDbContext _db;

    public FileRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(FileItem file, CancellationToken ct)
    {
        _db.Files.Add(file);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<FileItem?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Files.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<FileItem>> ListAsync(string ownerUserId, string? search, CancellationToken ct)
    {
        var q = _db.Files.AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(x => x.OriginalName.ToLower().Contains(search.ToLower()));

        return await q.OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
    }

    public async Task DeleteAsync(FileItem file, CancellationToken ct)
    {
        _db.Files.Remove(file);
        await _db.SaveChangesAsync(ct);
    }
}
