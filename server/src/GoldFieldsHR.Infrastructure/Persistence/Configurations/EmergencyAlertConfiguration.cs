using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class EmergencyAlertConfiguration : IEntityTypeConfiguration<EmergencyAlert>
{
    public void Configure(EntityTypeBuilder<EmergencyAlert> builder)
    {
        builder.ToTable("EmergencyAlerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Message)
            .HasMaxLength(1000);

        builder.Property(a => a.ResolutionNotes)
            .HasMaxLength(1000);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.Status);
    }
}
