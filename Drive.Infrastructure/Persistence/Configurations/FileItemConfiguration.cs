using Drive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drive.Infrastructure.Persistence.Configurations;

public class FileItemConfiguration : IEntityTypeConfiguration<FileItem>
{
    public void Configure(EntityTypeBuilder<FileItem> builder)
    {
        builder.ToTable("files");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalName)
            .HasColumnName("original_name")
            .IsRequired();

        builder.Property(x => x.StoredKey)
            .HasColumnName("stored_key")
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasColumnName("content_type")
            .IsRequired();

        builder.Property(x => x.SizeInBytes)
            .HasColumnName("size_bytes")
            .IsRequired();

        builder.Property(x => x.OwnerUserId)
            .HasColumnName("owner_user_id")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();
    }
}
