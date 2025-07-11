using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoldenFiberERP.Domain.Entities.Settings;

namespace GoldenFiberERP.Infrastructure.Persistence.Configurations.Settings;

/// <summary>
/// Entity Framework configuration for Country entity
/// </summary>
public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        // Table name will be automatically converted to snake_case by PostgreSqlConfiguration
        builder.ToTable("Countries");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(2)
            .IsFixedLength();

        builder.Property(c => c.Code3)
            .HasMaxLength(3)
            .IsFixedLength();

        builder.Property(c => c.NumericCode)
            .HasMaxLength(3)
            .IsFixedLength();

        builder.Property(c => c.PhoneCode)
            .HasMaxLength(5);

        builder.Property(c => c.Capital)
            .HasMaxLength(100);

        builder.Property(c => c.CurrencyCode)
            .HasMaxLength(3);

        builder.Property(c => c.CurrencySymbol)
            .HasMaxLength(5);

        builder.Property(c => c.TimeZone)
            .HasMaxLength(50);

        builder.Property(c => c.Region)
            .HasMaxLength(50);

        builder.Property(c => c.SubRegion)
            .HasMaxLength(50);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Audit fields (inherited from AuditableEntity)
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasDefaultValue(0);

        builder.Property(c => c.UpdatedBy)
            .HasDefaultValue(null);

        // Indexes for performance
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("IX_Countries_Code");

        builder.HasIndex(c => c.Code3)
            .IsUnique()
            .HasDatabaseName("IX_Countries_Code3");

        builder.HasIndex(c => c.NumericCode)
            .IsUnique()
            .HasDatabaseName("IX_Countries_NumericCode");

        builder.HasIndex(c => c.Name)
            .HasDatabaseName("IX_Countries_Name");

        builder.HasIndex(c => c.Region)
            .HasDatabaseName("IX_Countries_Region");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Countries_IsActive");

        builder.HasIndex(c => new { c.IsActive, c.DisplayOrder, c.Name })
            .HasDatabaseName("IX_Countries_Active_Display_Name");

        // Domain events are ignored in database mapping
        builder.Ignore(c => c.DomainEvents);
    }
}
