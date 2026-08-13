using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class LegalAppointmentConfiguration : IEntityTypeConfiguration<LegalAppointment>
{
    public void Configure(EntityTypeBuilder<LegalAppointment> builder)
    {
        builder.ToTable("LegalAppointments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.AppointedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.RejectionReason)
            .HasMaxLength(500);

        builder.Property(p => p.RevokedNotes)
            .HasMaxLength(1000);

        builder.Property(p => p.AppointmentType)
            .HasConversion<string>()
            .HasMaxLength(40);

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
