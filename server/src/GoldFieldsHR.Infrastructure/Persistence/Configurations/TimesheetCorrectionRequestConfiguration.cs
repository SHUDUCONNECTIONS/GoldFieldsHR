using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class TimesheetCorrectionRequestConfiguration : IEntityTypeConfiguration<TimesheetCorrectionRequest>
{
    public void Configure(EntityTypeBuilder<TimesheetCorrectionRequest> builder)
    {
        builder.ToTable("TimesheetCorrectionRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(500);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(r => r.TimesheetEntry)
            .WithMany()
            .HasForeignKey(r => r.TimesheetEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Employee)
            .WithMany()
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.Status);
    }
}
