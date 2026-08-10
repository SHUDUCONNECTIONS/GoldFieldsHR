using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class PpeRequestConfiguration : IEntityTypeConfiguration<PpeRequest>
{
    public void Configure(EntityTypeBuilder<PpeRequest> builder)
    {
        builder.ToTable("PpeRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Size)
            .HasMaxLength(20);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(500);

        builder.Property(r => r.ItemType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(r => r.Employee)
            .WithMany()
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Status);
    }
}
