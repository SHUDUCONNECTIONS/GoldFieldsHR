using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class AcknowledgmentConfiguration : IEntityTypeConfiguration<Acknowledgment>
{
    public void Configure(EntityTypeBuilder<Acknowledgment> builder)
    {
        builder.ToTable("Acknowledgments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EntityType, a.EntityId });

        // A given HR/Executive reviewer can only acknowledge the same record once.
        builder.HasIndex(a => new { a.EntityType, a.EntityId, a.EmployeeId }).IsUnique();
    }
}
