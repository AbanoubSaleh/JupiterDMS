using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JupiterDMS.Infrastructure.DataAccess.Configurations;

/// <summary>
/// Entity configuration for Folder entity.
/// </summary>
public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    /// <summary>
    /// Configures the Folder entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.ToTable("Folders");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(DomainConstants.Folder.NameMaxLength);

        builder.Property(f => f.Path)
            .IsRequired()
            .HasMaxLength(DomainConstants.Folder.PathMaxLength);

        builder.Property(f => f.LibraryId)
            .IsRequired();

        builder.Property(f => f.CreatedOn)
            .IsRequired();

        builder.Property(f => f.CreatedBy)
            .IsRequired();

        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(f => f.LibraryId);
        builder.HasIndex(f => f.ParentFolderId);
        builder.HasIndex(f => f.Path);
        builder.HasIndex(f => f.IsDeleted);

        builder.HasOne(f => f.Library)
            .WithMany(l => l.Folders)
            .HasForeignKey(f => f.LibraryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.ParentFolder)
            .WithMany(f => f.ChildFolders)
            .HasForeignKey(f => f.ParentFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Documents)
            .WithOne(d => d.Folder)
            .HasForeignKey(d => d.FolderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

