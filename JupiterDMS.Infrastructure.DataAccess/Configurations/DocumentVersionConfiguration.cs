using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JupiterDMS.Infrastructure.DataAccess.Configurations;

/// <summary>
/// Entity configuration for DocumentVersion entity.
/// </summary>
public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    /// <summary>
    /// Configures the DocumentVersion entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("DocumentVersions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.DocumentId)
            .IsRequired();

        builder.Property(v => v.VersionNumber)
            .IsRequired();

        builder.Property(v => v.FilePath)
            .IsRequired()
            .HasMaxLength(DomainConstants.Document.FilePathMaxLength);

        builder.Property(v => v.FileSizeBytes)
            .IsRequired();

        builder.Property(v => v.Comment)
            .HasMaxLength(1000);

        builder.Property(v => v.CreatedOn)
            .IsRequired();

        builder.Property(v => v.CreatedBy)
            .IsRequired();

        builder.Property(v => v.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(v => v.DocumentId);
        builder.HasIndex(v => new { v.DocumentId, v.VersionNumber }).IsUnique();

        builder.HasOne(v => v.Document)
            .WithMany(d => d.Versions)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

