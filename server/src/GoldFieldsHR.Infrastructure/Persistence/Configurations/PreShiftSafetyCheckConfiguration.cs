using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class PreShiftSafetyCheckConfiguration : IEntityTypeConfiguration<PreShiftSafetyCheck>
{
    public void Configure(EntityTypeBuilder<PreShiftSafetyCheck> builder)
    {
        builder.ToTable("PreShiftSafetyChecks");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.HazardNotes)
            .HasMaxLength(1000);

        builder.HasOne(c => c.Employee)
            .WithMany()
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.EmployeeId, c.CheckDate })
            .IsUnique();
    }
}
