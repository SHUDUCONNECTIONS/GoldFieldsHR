using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class ShiftChangeRequestConfiguration : IEntityTypeConfiguration<ShiftChangeRequest>
{
    public void Configure(EntityTypeBuilder<ShiftChangeRequest> builder)
    {
        builder.ToTable("ShiftChangeRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Comments)
            .HasMaxLength(500);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(500);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(r => r.RequestedShiftType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(r => r.Employee)
            .WithMany()
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Status);
    }
}
