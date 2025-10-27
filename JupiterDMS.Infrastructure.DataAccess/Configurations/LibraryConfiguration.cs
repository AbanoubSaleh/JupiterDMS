using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JupiterDMS.Infrastructure.DataAccess.Configurations;

/// <summary>
/// Entity configuration for Library entity.
/// </summary>
public class LibraryConfiguration : IEntityTypeConfiguration<Library>
{
    /// <summary>
    /// Configures the Library entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Library> builder)
    {
        builder.ToTable("Libraries");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(DomainConstants.Library.NameMaxLength);

        builder.Property(l => l.Description)
            .HasMaxLength(DomainConstants.Library.DescriptionMaxLength);

        builder.Property(l => l.IsActive)
            .IsRequired();

        builder.Property(l => l.CreatedOn)
            .IsRequired();

        builder.Property(l => l.CreatedBy)
            .IsRequired();

        builder.Property(l => l.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(l => l.Name);
        builder.HasIndex(l => l.IsDeleted);

        builder.HasMany(l => l.Folders)
            .WithOne(f => f.Library)
            .HasForeignKey(f => f.LibraryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

