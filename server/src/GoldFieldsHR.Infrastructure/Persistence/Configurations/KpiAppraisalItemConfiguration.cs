using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class KpiAppraisalItemConfiguration : IEntityTypeConfiguration<KpiAppraisalItem>
{
    public void Configure(EntityTypeBuilder<KpiAppraisalItem> builder)
    {
        builder.ToTable("KpiAppraisalItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.DescriptionSnapshot)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.CategoryNameSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.SubGroupLabelSnapshot)
            .HasMaxLength(100);

        builder.Property(i => i.Checkpoint1Comment).HasMaxLength(1000);
        builder.Property(i => i.Checkpoint2Comment).HasMaxLength(1000);
        builder.Property(i => i.Checkpoint3Comment).HasMaxLength(1000);
        builder.Property(i => i.Checkpoint4Comment).HasMaxLength(1000);
        builder.Property(i => i.Evaluation).HasMaxLength(1000);

        builder.HasOne(i => i.KpiTemplateItem)
            .WithMany()
            .HasForeignKey(i => i.KpiTemplateItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.KpiAppraisalId);
    }
}
