using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class MedicalExaminationConfiguration : IEntityTypeConfiguration<MedicalExamination>
{
    public void Configure(EntityTypeBuilder<MedicalExamination> builder)
    {
        builder.ToTable("MedicalExaminations");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(m => m.Restrictions)
            .HasMaxLength(500);

        builder.Property(m => m.Notes)
            .HasMaxLength(1000);

        builder.HasOne(m => m.Employee)
            .WithMany()
            .HasForeignKey(m => m.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.ExaminedByEmployee)
            .WithMany()
            .HasForeignKey(m => m.ExaminedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.ExpiryDate);
    }
}
