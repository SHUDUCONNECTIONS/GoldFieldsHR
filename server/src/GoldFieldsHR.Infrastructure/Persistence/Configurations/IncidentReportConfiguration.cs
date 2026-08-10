using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class IncidentReportConfiguration : IEntityTypeConfiguration<IncidentReport>
{
    public void Configure(EntityTypeBuilder<IncidentReport> builder)
    {
        builder.ToTable("IncidentReports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.ReviewNotes)
            .HasMaxLength(1000);

        builder.Property(r => r.Severity)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(r => r.Employee)
            .WithMany()
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Status);
    }
}
