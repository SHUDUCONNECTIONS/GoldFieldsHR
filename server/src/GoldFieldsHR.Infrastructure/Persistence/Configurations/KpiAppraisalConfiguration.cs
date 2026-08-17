using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class KpiAppraisalConfiguration : IEntityTypeConfiguration<KpiAppraisal>
{
    public void Configure(EntityTypeBuilder<KpiAppraisal> builder)
    {
        builder.ToTable("KpiAppraisals");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.PeriodLabel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.InductionNumber)
            .HasMaxLength(50);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.KpiTemplate)
            .WithMany()
            .HasForeignKey(a => a.KpiTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Items)
            .WithOne(i => i.KpiAppraisal)
            .HasForeignKey(i => i.KpiAppraisalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.EmployeeId);
        builder.HasIndex(a => a.Status);
    }
}
