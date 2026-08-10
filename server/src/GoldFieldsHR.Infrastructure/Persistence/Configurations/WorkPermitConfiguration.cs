using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class WorkPermitConfiguration : IEntityTypeConfiguration<WorkPermit>
{
    public void Configure(EntityTypeBuilder<WorkPermit> builder)
    {
        builder.ToTable("WorkPermits");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.RejectionReason)
            .HasMaxLength(500);

        builder.Property(p => p.ClosedNotes)
            .HasMaxLength(1000);

        builder.Property(p => p.PermitType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.Status);
    }
}
