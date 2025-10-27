using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JupiterDMS.Infrastructure.DataAccess.Configurations;

/// <summary>
/// Entity configuration for Document entity.
/// </summary>
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    /// <summary>
    /// Configures the Document entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(DomainConstants.Document.NameMaxLength);

        builder.Property(d => d.FilePath)
            .IsRequired()
            .HasMaxLength(DomainConstants.Document.FilePathMaxLength);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(DomainConstants.Document.ContentTypeMaxLength);

        builder.Property(d => d.FileExtension)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.CurrentVersion)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(d => d.FileSizeBytes)
            .IsRequired();

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.FileType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.CheckoutStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.CheckoutStatus.Available);

        builder.Property(d => d.CheckedOutBy)
            .IsRequired(false);

        builder.Property(d => d.CheckedOutOn)
            .IsRequired(false);

        builder.Property(d => d.CheckoutExpiry)
            .IsRequired(false);

        builder.Property(d => d.Title)
            .IsRequired(false)
            .HasMaxLength(DomainConstants.Document.TitleMaxLength);

        builder.Property(d => d.Description)
            .IsRequired(false)
            .HasMaxLength(DomainConstants.Document.DescriptionMaxLength);

        builder.Property(d => d.Tags)
            .IsRequired(false)
            .HasMaxLength(DomainConstants.Document.TagsMaxLength);

        builder.Property(d => d.FolderId)
            .IsRequired();

        builder.Property(d => d.CreatedOn)
            .IsRequired();

        builder.Property(d => d.CreatedBy)
            .IsRequired();

        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(d => d.FolderId);
        builder.HasIndex(d => d.Name);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.FileType);
        builder.HasIndex(d => d.CheckoutStatus);
        builder.HasIndex(d => d.CheckedOutBy);
        builder.HasIndex(d => d.Title);
        builder.HasIndex(d => d.IsDeleted);

        builder.HasOne(d => d.Folder)
            .WithMany(f => f.Documents)
            .HasForeignKey(d => d.FolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Versions)
            .WithOne(v => v.Document)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

