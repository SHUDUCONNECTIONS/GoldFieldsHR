using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class PolicyAcknowledgmentConfiguration : IEntityTypeConfiguration<PolicyAcknowledgment>
{
    public void Configure(EntityTypeBuilder<PolicyAcknowledgment> builder)
    {
        builder.ToTable("PolicyAcknowledgments");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Policy)
            .WithMany(p => p.Acknowledgments)
            .HasForeignKey(a => a.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.PolicyId, a.EmployeeId })
            .IsUnique();
    }
}
