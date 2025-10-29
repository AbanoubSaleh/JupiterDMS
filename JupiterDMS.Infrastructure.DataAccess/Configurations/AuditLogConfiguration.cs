using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JupiterDMS.Infrastructure.DataAccess.Configurations;

/// <summary>
/// Entity configuration for AuditLog entity.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <summary>
    /// Configures the AuditLog entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ActionType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(DomainConstants.AuditLog.EntityTypeMaxLength);

        builder.Property(a => a.EntityId)
            .IsRequired();

        builder.Property(a => a.UserEmail)
            .IsRequired()
            .HasMaxLength(DomainConstants.User.EmailMaxLength);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(DomainConstants.AuditLog.DescriptionMaxLength);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.CreatedOn)
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .IsRequired();

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(a => a.UserEmail);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.ActionType);


    }
}

